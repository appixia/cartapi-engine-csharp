using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Appixia.Engine.Mediums;
using System.IO;

namespace Appixia.Engine.Mediums.JSON
{
    public class Encoder : Appixia.Engine.Mediums.Encoder
    {
        public override string ContentType { get { return "text/javascript"; } }

        public override async Task RenderAsync(HttpResponse res, TextWriter output, Container root)
        {
            await RenderHeaderAsync(res, output);
            await RenderFieldAsync(res, output, root, null);
            await RenderFooterAsync(res, output);
        }

        protected virtual async Task RenderHeaderAsync(HttpResponse res, TextWriter output)
        {
            res.ContentType = ContentType;
            await output.WriteAsync("");
        }

        protected virtual async Task RenderFooterAsync(HttpResponse res, TextWriter output)
        {
            await output.WriteAsync("");
        }

        protected virtual async Task RenderFieldHeaderAsync(HttpResponse res, TextWriter output, string fieldname, string newline)
	    {
		    if (fieldname != null) await output.WriteAsync("\"" + fieldname + "\":" + newline);
	    }

        protected virtual async Task RenderFieldFooterAsync(HttpResponse res, TextWriter output, string fieldname, string newline)
	    {
            if (fieldname != null) await output.WriteAsync(newline);
	    }

        protected virtual async Task RenderFieldAsync(HttpResponse res, TextWriter output, object fieldvalue, string fieldname)
	    {
		    if (fieldvalue is Appixia.Engine.Mediums.Array)
		    {
			    await RenderFieldHeaderAsync(res, output, fieldname, "");
			    await output.WriteAsync("[");
			    bool first = true;
			    foreach (object fieldvalue2 in ((Appixia.Engine.Mediums.Array)fieldvalue))
			    {
				    if (first) first = false;
                    else await output.WriteAsync(",");
                    await RenderFieldAsync(res, output, fieldvalue2, null);
			    }
                await output.WriteAsync("]");
                await RenderFieldFooterAsync(res, output, fieldname, "");
		    }
		    else if (fieldvalue is Container)
		    {
                await RenderFieldHeaderAsync(res, output, fieldname, "");
                await output.WriteAsync("{");
			    bool first = true;
			    foreach (string fieldname2 in ((Container)fieldvalue).Keys)
			    {
				    if (first) first = false;
                    else await output.WriteAsync(",");
				    object fieldvalue2 = ((Container)fieldvalue).Get(fieldname2);
                    await RenderFieldAsync(res, output, fieldvalue2, fieldname2);
			    }
                await output.WriteAsync("}");
                await RenderFieldFooterAsync(res, output, fieldname, "");
		    }
		    else
		    {
                await RenderFieldHeaderAsync(res, output, fieldname, "");
                await RenderStringAsync(res, output, fieldvalue.ToString(), "");
                await RenderFieldFooterAsync(res, output, fieldname, "");
		    }
	    }

        protected virtual async Task RenderStringAsync(HttpResponse res, TextWriter output, object obj, string newline)
	    {
		    if (obj is bool)
		    {
                await output.WriteAsync(obj.ToString() + newline);
		    }
		    else if (obj is ValueType)
		    {
                await output.WriteAsync(obj.ToString() + newline);
		    }
		    else
		    {
			    string str = obj.ToString();
			    str = str.Replace("\"", "\\\"");
                await output.WriteAsync("\"" + str + "\"" + newline);
		    }
	    }
    }
}
