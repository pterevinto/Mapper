using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;

namespace Mapper
{
    public static class Mapper
    {

        public static dynamic Map(this object Origin, Type destinationType, Dictionary<string, string> ExplicitMap = null)
        {

            object Destination = null;

            Destination = Activator.CreateInstance(destinationType);

            PropertyInfo[] OriginProperties = Origin.GetType().GetProperties();
            PropertyInfo[] DestinationProperties = Destination.GetType().GetProperties();

            foreach (PropertyInfo tmpProp in DestinationProperties)
            {
                if (OriginProperties.Where(P => P.Name == tmpProp.Name).Count() > 0)
                {
                    PropertyInfo OriginProp = OriginProperties.Where(P => P.Name == tmpProp.Name).First();
                    Type OriginPropertyType = OriginProp.PropertyType;

                    if (OriginPropertyType.IsGenericType && OriginPropertyType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        Type DestinationItemType = tmpProp.GetType().GetGenericArguments()[0];
                        Type OriginItemType = OriginPropertyType.GetGenericArguments()[0];

                        var listInstance = (IList)typeof(List<>).MakeGenericType(DestinationItemType).GetConstructor(Type.EmptyTypes).Invoke(null);

                        foreach (object OriginItem in (IList)OriginProp.GetValue(Origin, null))
                        {
                            var DestinationType = OriginItem.Map(DestinationItemType);
                            listInstance.Add(DestinationType);
                        }

                        tmpProp.SetValue(Destination, listInstance, null);
                    }
                    else
                    {
                        if (OriginProp.PropertyType == tmpProp.PropertyType)
                        {
                            tmpProp.SetValue(Destination, OriginProp.GetValue(Origin, null), null);
                        }
                    }
                }
            }

            if (ExplicitMap != null)
                foreach (string OriginProp in ExplicitMap.Keys)
                {
                    if (OriginProperties.Where(P => P.Name == OriginProp).Count() > 0 && DestinationProperties.Where(P => P.Name == ExplicitMap[OriginProp]).Count() > 0)
                    {
                        PropertyInfo valorOrigen = OriginProperties.Where(P => P.Name == OriginProp).First();
                        PropertyInfo valorDestino = DestinationProperties.Where(P => P.Name == ExplicitMap[OriginProp]).First();

                        valorDestino.SetValue(Destination, valorOrigen.GetValue(Origin, null), null);
                    }
                }


            return Destination;
        }

        public static void InverseMap(this object Destination, object Origin, Dictionary<string, string> ExplicitMap = null)
        {

            Destination = Origin.Map(Destination.GetType(), ExplicitMap);

        }

    }
}
