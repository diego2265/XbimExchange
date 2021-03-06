﻿using Xbim.Ifc2x3.ProductExtension;

namespace Xbim.COBieLite
{
    public partial class ZoneType
    {
        public ZoneType()
        {
            
        }

        public ZoneType(IfcZone ifcZone, CoBieLiteHelper helper)
            : this()
        {
            externalEntityName = helper.ExternalEntityName(ifcZone);
            externalID = helper.ExternalEntityIdentity(ifcZone);
            externalSystemName = helper.ExternalSystemName(ifcZone);
            ZoneDescription = ifcZone.Description;
            ZoneCategory = helper.GetClassification(ifcZone);
            ZoneName = ifcZone.Name;
            //Attributes
            AttributeType[] ifcAttributes = helper.GetAttributes(ifcZone);
            if (ifcAttributes != null && ifcAttributes.Length > 0)
                ZoneAttributes = new AttributeCollectionType { Attribute = ifcAttributes };

            //TODO:
            //Space Issues
            //Space Documents
        }
    }
}
