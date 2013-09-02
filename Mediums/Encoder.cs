using System;
using System.Threading.Tasks;
using System.Web;
using System.IO;

namespace Appixia.Engine.Mediums
{
    public abstract class Encoder
    {
        public virtual Container CreateRoot()
	    {
		    return new Container();
	    }

        public abstract string ContentType { get; }

        public abstract Task RenderAsync(HttpResponse res, TextWriter output, Container root);

        public virtual void AddString(Container container, string fieldname, string str)
        {
            AddFieldAutoArray(container, fieldname, str);
        }

        public virtual void AddNumber(Container container, string fieldname, ValueType number)
        {
	        AddFieldAutoArray(container, fieldname, number);
        }

        public virtual void AddBoolean(Container container, string fieldname, bool boolean)
        {
	        AddFieldAutoArray(container, fieldname, boolean);
        }

        public virtual Container AddContainer(Container container, string fieldname)
        {
	        Container created = new Container();
	        AddFieldAutoArray(container, fieldname, created);
	        return created;
        }

        public virtual Array AddArray(Container container, string fieldname)
        {
	        Array created = new Array();
            container.Put(fieldname, created);
	        return created;
        }

        public virtual void AddStringToArray(Array array, string str)
        {
            array.Add(str);
        }

        public virtual void AddNumberToArray(Array array, ValueType number)
        {
	        array.Add(number);
        }

        public virtual void AddBooleanToArray(Array array, bool boolean)
        {
	        array.Add(boolean);
        }

        public virtual Container AddContainerToArray(Array array)
        {
	        Container created = new Container();
	        array.Add(created);
	        return created;
        }

        public virtual void AddExistingContainer(Container toContainer, string fieldname, Container addedContainer)
        {
            AddFieldAutoArray(toContainer, fieldname, addedContainer);
        }

        public virtual void AddExistingContainerToArray(Array toArray, Container addedContainer)
        {
            toArray.Add(addedContainer);
        }

        public virtual void AddExistingArray(Container toContainer, string fieldname, Array addedArray)
        {
            toContainer.Put(fieldname, addedArray); // no support for AddFieldAutoArray in this case
        }

        public virtual void AddFieldAutoArray(Container container, string fieldname, object fieldvalue)
        {
	        if (!container.ContainsKey(fieldname))
	        {
		        container.Put(fieldname, fieldvalue);
	        }
	        else
	        {
		        object existing = container.Get(fieldname);
		        if (existing is Array)
		        {
			        ((Array)existing).Add(fieldvalue);
		        }
		        else
		        {
			        Array array = new Array();
			        array.Add(existing);
			        array.Add(fieldvalue);
			        container.Put(fieldname, array);
		        }
	        }
        }
    }
}
