using System.Collections.Generic;
using System.Threading.Tasks;
using Appixia.Engine.Mediums;
using System.Web;
using System;
using System.IO;

namespace Appixia.Engine
{
    public class DieException : Exception { };

    public static class Helpers
    {
        public static Container CreateSuccessResponse(Encoder encoder)
	    {
		    var root = encoder.CreateRoot();
		    var result = encoder.AddContainer(root, "Result");
		    encoder.AddBoolean(result, "Success", true);
		    return root;
	    }

	    public static Container CreateSuccessResponseWithPaging(Encoder encoder, Container paging_request, int total_elements)
	    {
		    var root = encoder.CreateRoot();
		    var result = encoder.AddContainer(root, "Result");
		    encoder.AddBoolean(result, "Success", true);
		    var paging = encoder.AddContainer(root, "Paging");
		    encoder.AddNumber(paging, "PageNumber", IntValue(paging_request.Get("PageNumber")));
		    encoder.AddNumber(paging, "ElementsPerPage", IntValue(paging_request.Get("ElementsPerPage")));

		    if (total_elements >= 0)
		    {
			    encoder.AddNumber(paging, "TotalPages", (int)Math.Ceiling((double)total_elements / FloatValue(paging_request.Get("ElementsPerPage"))));
			    encoder.AddNumber(paging, "TotalElements", total_elements);
		    }
		
		    return root;
	    }

	    public static async Task DieOnErrorAsync(HttpResponse res, TextWriter output, Encoder encoder, string error, string message)
	    {
		    var root = encoder.CreateRoot();
		    var result = encoder.AddContainer(root, "Result");
		    encoder.AddBoolean(result, "Success", false);
		    Appixia.Engine.Mediums.Array details = encoder.AddArray(result, "Detail");
		    var detail = encoder.AddContainerToArray(details);
		    encoder.AddString(detail, "Error", error);
		    encoder.AddString(detail, "Message", message);
		    await encoder.RenderAsync(res, output, root);
            await output.FlushAsync();
            throw new DieException();
	    }

	    public static int GetZbOffsetFromPagingRequest(Container paging_request)
	    {
		    int page_number = IntValue(paging_request.Get("PageNumber"));
		    int elements_per_page = IntValue(paging_request.Get("ElementsPerPage"));
		    int first_index_zb = (page_number - 1) * elements_per_page;
		    return first_index_zb;
	    }

	    public static async Task ValidatePagingRequestAsync(HttpResponse res, TextWriter output, Encoder encoder, object paging_request_object)
	    {
            if (!(paging_request_object is Container)) await Helpers.DieOnErrorAsync(res, output, encoder, "InvalidRequest", "PagingRequest is invalid");
		    var paging_request = (Container)paging_request_object;
            if (!paging_request.ContainsKey("PageNumber")) await Helpers.DieOnErrorAsync(res, output, encoder, "IncompleteRequest", "PagingRequest.PageNumber missing");
		    object page_number = paging_request.Get("PageNumber");
            if (!IsNumeric(page_number)) await Helpers.DieOnErrorAsync(res, output, encoder, "InvalidRequest", "PagingRequest.PageNumber not numeric");
            if (IntValue(page_number) < 1) await Helpers.DieOnErrorAsync(res, output, encoder, "PageOutOfBounds", "PagingRequest.PageNumber below 1");
            if (!paging_request.ContainsKey("ElementsPerPage")) await Helpers.DieOnErrorAsync(res, output, encoder, "IncompleteRequest", "PagingRequest.ElementsPerPage missing");
		    object elements_per_page = paging_request.Get("ElementsPerPage");
            if (!IsNumeric(elements_per_page)) await Helpers.DieOnErrorAsync(res, output, encoder, "InvalidRequest", "PagingRequest.ElementsPerPage not numeric");
            if (IntValue(elements_per_page) < 1) await Helpers.DieOnErrorAsync(res, output, encoder, "PageSizeInvalid", "PagingRequest.ElementsPerPage below 1");
	    }

	    public static async Task ValidateFilterAsync(HttpResponse res, TextWriter output, Encoder encoder, object filter_object, Dictionary<string,string> db_field_name_map)
	    {
            if (!(filter_object is Container)) await Helpers.DieOnErrorAsync(res, output, encoder, "InvalidRequest", "Filter is invalid");
		    var filter = (Container)filter_object;
            if (!filter.ContainsKey("Field")) await Helpers.DieOnErrorAsync(res, output, encoder, "IncompleteRequest", "Filter.Field missing");
            if (!filter.ContainsKey("Relation")) await Helpers.DieOnErrorAsync(res, output, encoder, "IncompleteRequest", "Filter.Relation missing");
            if (!filter.ContainsKey("Value")) await Helpers.DieOnErrorAsync(res, output, encoder, "IncompleteRequest", "Filter.Value missing");
		    if (db_field_name_map != null)
		    {
                if (!db_field_name_map.ContainsKey(filter.Get("Field").ToString())) await Helpers.DieOnErrorAsync(res, output, encoder, "UnsupportedFilter", filter.Get("Field").ToString() + " filter is unsupported");
		    }
	    }

        public static Appixia.Engine.Mediums.Array GetDictionaryKeyAsArray(Container dictionary, string key)
        {
            object obj = dictionary.Get(key);
            if (obj == null) return new Appixia.Engine.Mediums.Array();
            if (obj is Appixia.Engine.Mediums.Array) return (Appixia.Engine.Mediums.Array)obj;
            return new Appixia.Engine.Mediums.Array() { obj };
        }
	
	    public static bool IsNumeric(object obj)  
	    {
            double d = 0;
            if (obj == null) return false;
		    return Double.TryParse(obj.ToString(), out d);
	    }

	    public static int IntValue(object obj)
	    {
		    return (int)FloatValue(obj);
	    }

	    public static double FloatValue(object obj)
	    {
		    double d = 0;
            if (obj == null) return d;
		    Double.TryParse(obj.ToString(), out d);
		    return d;
	    }
    }
}
