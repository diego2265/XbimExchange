﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Xbim.COBieLite;
using Xbim.Ifc2x3.Kernel;
using Xbim.Ifc2x3.ProductExtension;
using Xbim.Ifc2x3.PropertyResource;
using Xbim.IO;

namespace XbimExchanger.COBieLiteToIfc
{
    public class CoBieLiteToIfcExchanger : XbimExchanger<FacilityType, XbimModel>
    {
        public struct NamedProperty
        {
            public string PropertySetName;
            public string PropertyName;

            public NamedProperty(string propertyName, string propertySetName) : this()
            {
                PropertyName = propertyName;
                PropertySetName = propertySetName;
            }
        }

        static readonly IDictionary<string, NamedProperty> CobieToIfcPropertyMap = new Dictionary<string, NamedProperty>();
        private readonly Dictionary<IfcObject, IfcPropertySetDefinition> _objectsToPropertySets = new Dictionary<IfcObject, IfcPropertySetDefinition>();

        static CoBieLiteToIfcExchanger()
        {
           
            var configMap = new ExeConfigurationFileMap { ExeConfigFilename = "COBieAttributes.config" };
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            var cobiePropertyMaps = (AppSettingsSection)config.GetSection("COBiePropertyMaps");

            foreach (KeyValueConfigurationElement keyVal in cobiePropertyMaps.Settings)
            {
                var values = keyVal.Value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                var selected = values.FirstOrDefault();
                if (selected != null)
                {
                    var names = selected.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
                    if(names.Length==2)
                        CobieToIfcPropertyMap.Add(keyVal.Key, new NamedProperty(names[0], names[1]));
                }
            }
            
        }

        public IfcPropertySingleValue CreatePropertySingleValue(CobieBaseType theSource, PropertyInfo theProperty, IfcObject ifcObject)
        {
            NamedProperty namedProperty;
            if(CobieToIfcPropertyMap.TryGetValue(theProperty.Name,out namedProperty))
            {
                IfcPropertySetDefinition propertySetDefinition;
                if (_objectsToPropertySets.TryGetValue(ifcObject, out propertySetDefinition)) //we need to create a set
                {
                    bool isQuantity = namedProperty.PropertySetName.StartsWith("qto", true,CultureInfo.InvariantCulture);
                    if (isQuantity)
                    {
                        var psetDef = TargetRepository.Instances.New<IfcElementQuantity>();
                    }
                    else
                    {
                        var psetDef = TargetRepository.Instances.New<IfcPropertySet>();
                    }

                }
            }
            throw new ArgumentException("Incorrect property map", "theProperty");
        }

        public CoBieLiteToIfcExchanger(FacilityType facility, XbimModel repository) : base(facility, repository)
        {
            LoadPropertySetDefinitions();
        }

        private void LoadPropertySetDefinitions()
        {
            var relProps = TargetRepository.Instances.OfType<IfcRelDefinesByProperties>().ToList();
            foreach (var relProp in relProps)
            {
                foreach (var ifcObject in relProp.RelatedObjects)
                {
                    IfcPropertySetDefinition propDefinition;
                    if (!_objectsToPropertySets.TryGetValue(ifcObject, out propDefinition))
                    {
                        _objectsToPropertySets.Add(ifcObject, propDefinition);
                    }
                    
                }
            }
        }

        public IfcBuilding Convert(FacilityType facility)
        {
            var mapping = GetOrCreateMappings<MappingFacilityTypeToIfcBuilding>();
            var building = mapping.GetOrCreateTargetObject(facility.externalID);
            return  mapping.AddMapping(facility, building);
            
        }

        public override XbimModel Convert()
        {
            Convert(SourceRepository);
            return TargetRepository;
        }
    }
}
