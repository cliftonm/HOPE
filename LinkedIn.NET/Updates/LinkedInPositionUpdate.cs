using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for PRFU updates
    /// </summary>
    public class LinkedInPositionUpdate : LinkedInUpdate
    {
        internal LinkedInPositionUpdate()
        {
        }

        private readonly List<string> _UpdateFields = new List<string>();

        /// <summary>
        /// Gets <see cref="LinkedInPersonPosition"/> object representing update's person
        /// </summary>
        public LinkedInPersonPosition Person { get; internal set; }

        /// <summary>
        /// Gets collection of strings representig update's updated fields
        /// </summary>
        public IEnumerable<string> UpdateFields
        {
            get { return _UpdateFields.AsEnumerable(); }
        }

        internal void AddUpdateFields(IEnumerable<string> updateFields)
        {
            _UpdateFields.AddRange(updateFields);
        }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent != null)
            {
                //person
                var xe = xcontent.Element("person");
                if (xe != null)
                {
                    Person = new LinkedInPersonPosition();
                    Utils.BuildPerson(Person, xe);
                    //positions
                    xe = xcontent.Element("positions");
                    if (xe != null)
                    {
                        Person.AddPositions(xe.Elements("position").Select(Utils.BuildPositionBase));
                    }
                }
            }
            //updated-fields
            var xfields = xp.Element("updated-fields");
            if (xfields != null)
            {
                AddUpdateFields(xfields.Elements("update-field").Select(xf =>
                {
                    var xElement = xf.Element("name");
                    return xElement != null ? xElement.Value.Trim() : null;
                }));
            }
        }
    }
}
