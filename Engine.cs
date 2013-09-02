using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Appixia.Engine.Mediums;
using System.IO;

namespace Appixia.Engine
{
    public static class Engine
    {
        public static string[] REQUEST_METADATA_FIELDS = {"X-OPERATION","X-VERSION","X-TOKEN","X-FORMAT","X-LANGUAGE","X-CURRENCY","X-FORMFACTOR"};

        // returns null on error
	    public static async Task<Request> HandleRequestAsync(HttpRequest req, HttpResponse res, TextWriter output)
	    {
		    var request = new Request();

		    // parse the request metadata
		    request.Metadata = GetRequestMetadata(req);

		    // parse the request post data (if found)
		    request.Data = new Container();
		    /*
		    $post_data = CartAPI_Engine::getRequestPostData();
		    if ($post_data !== false)
		    {
			    $decoder = CartAPI_Engine::getDecoder($request['metadata']['X-FORMAT']);
			    if ($decoder !== false) $request['data'] = $decoder->parse($post_data);
		    }
		    */

		    // override with parameters passed on the URL
		    ParseUrlRequestData(req, request.Data);

		    // prepare an encoder for the response
		    request.Encoder = GetEncoder(request.Metadata["X-FORMAT"]);
            if (request.Encoder == null) return null;

		    // do some sanity checking
		    if (!request.Metadata.ContainsKey("X-OPERATION")) await Helpers.DieOnErrorAsync(res, output, request.Encoder, "IncompleteMetadata", "X-OPERATION missing from metadata");
		
		    return request;
	    }

        public static Encoder GetEncoder(string medium)
        {
            return (Encoder)_newMediumClass(medium, "Encoder");
        }

        /*
	    public static Decoder getDecoder(String medium)
	    {
		    return (Decoder)_newMediumClass(medium, "Decoder");
	    }
	    */

        public static Dictionary<string,string> GetRequestMetadata(HttpRequest req)
	    {
		    var res = new Dictionary<string,string>();

		    // defaults
		    res["X-FORMAT"] = "XML";
		    res["X-VERSION"] = "1";
		    res["X-LANGUAGE"] = "en";
		    res["X-CURRENCY"] = "USD";

		    // first look in HTTP headers
		    foreach (string field in REQUEST_METADATA_FIELDS)
		    {
			    // first check in HTTP headers
			    string[] from_header = req.Headers.GetValues(field);
			    if (from_header != null) res[field] = from_header[0];

			    // override with URL
			    string[] from_url = req.QueryString.GetValues(field);
			    if (from_url != null) res[field] = from_url[0];
		    }

		    return res;
	    }

        private static void PutValueInUnknownHolder(object holder, string field, object value)
        {
            if (holder is Container)
            {
                var container = (Container)holder;
                container.Put(field, value);
            }
            else if (holder is Appixia.Engine.Mediums.Array)
            {
                var array = (Appixia.Engine.Mediums.Array)holder;
                if (String.IsNullOrEmpty(field))
                {
                    // empty index, just add in the end of the array
                    array.Add(value);
                }
                else
                {
                    // a numeric index
                    int index = Helpers.IntValue(field);
                    if (index >= array.Count)
                    {
                        // we are adding a new value
                        while (array.Count < index) array.Add(null);
                        array.Add(value);
                    }
                    else if (index >= 0)
                    {
                        // we are changing an existing value
                        array[index] = value;
                    }
                }
            }
        }

        // returns null if not found
        private static object GetValueInUnknownHolder(object holder, string field)
        {
            if (holder is Container)
            {
                var container = (Container)holder;
                return container.Get(field);
            }
            else if (holder is Appixia.Engine.Mediums.Array)
            {
                var array = (Appixia.Engine.Mediums.Array)holder;
                if (String.IsNullOrEmpty(field)) return null;
                else
                {
                    // a numeric index
                    int index = Helpers.IntValue(field);
                    if (index >= array.Count) return null;
                    else if (index >= 0) return array[index];
                }
            }
            return null;
        }

        public static void ParseUrlRequestData(HttpRequest req, Container request_data)
        {
            string[] param_names = req.QueryString.AllKeys;
            for (int param_index = 0; param_index < param_names.Length; param_index++)
            {
                string param_name = param_names[param_index];
                if (param_name == null) continue;

                string[] param_values = req.QueryString.GetValues(param_name);

                // handle containers
                string[] name_parts = param_name.Replace("]","").Split('[');

                // iterate over the group
                for (int pvalue_index = 0; pvalue_index < param_values.Length; pvalue_index++)
                {
                    string param_value = param_values[pvalue_index];
                    object cur_holder = request_data;

                    for (int i = 0; i < name_parts.Length; i++)
                    {
                        if (i == (name_parts.Length - 1)) PutValueInUnknownHolder(cur_holder, name_parts[i], param_value);
                        else
                        {
                            object next_holder = GetValueInUnknownHolder(cur_holder, name_parts[i]);
                            if (next_holder != null)
                            {
                                // holder already exists, move on
                                cur_holder = next_holder;
                            }
                            else
                            {
                                // holder doesn't exist, need to create it, should it be an array or a container? let's rely on the next index
                                string next_field = name_parts[i + 1];
                                if (String.IsNullOrEmpty(next_field) || Helpers.IsNumeric(next_field)) next_holder = new Appixia.Engine.Mediums.Array();
                                else next_holder = new Container();
                                // put it
                                PutValueInUnknownHolder(cur_holder, name_parts[i], next_holder);
                                cur_holder = next_holder;
                            }
                        }
                    }
                }
            }
        }

        /*
	    // return null if none
	    public static function getRequestPostData()
	    {
		    if ($_SERVER["REQUEST_METHOD"] != "POST") return false;
		    $post_data = file_get_contents("php://input");
		    if (($post_data === false) || empty($post_data)) return false;
		    return $post_data;
	    }
	    */

        // return null on failure
	    private static object _newMediumClass(string medium, string class_type)
	    {
		    try
		    {
			    Type class_object = Type.GetType("Appixia.Engine.Mediums." + medium + "." + class_type);
                return Activator.CreateInstance(class_object);
		    }
		    catch (Exception)
		    {
			    return null;
		    }
	    }
    }
}
