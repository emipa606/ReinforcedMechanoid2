using System.Xml;
using Verse;

namespace ReinforcedMechanoids;

public class ThingDefRecord
{
    public float commonality;
    public ThingDef thing;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "thing", xmlRoot);
        commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
    }
}