using System.Linq;
using System.Xml.Linq;

namespace LinkedIn.NET.Updates
{
    /// <summary>
    /// Represents LinkedIn update object for JGRP updates
    /// </summary>
    public class LinkedInGroupUpdate : LinkedInUpdate
    {
        internal LinkedInGroupUpdate()
        {
        }

        /// <summary>
        /// Gets <see cref="LinkedInPersonGroup"/> object representing update's person
        /// </summary>
        public LinkedInPersonGroup Person { get; internal set; }

        internal override void BuildUpdate(XElement xp)
        {
            SetBaseValues(xp);

            //update-content
            var xcontent = xp.Element("update-content");
            if (xcontent == null) return;
            //person
            var xe = xcontent.Element("person");
            if (xe == null) return;
            Person = new LinkedInPersonGroup();
            Utils.BuildPerson(Person, xe);
            //member-groups
            xe = xe.Element("member-groups");
            if (xe != null)
            {
                Person.AddGroups(xe.Elements("member-group").Select(Utils.BuildMemberGroup));
            }
        }
    }
}
