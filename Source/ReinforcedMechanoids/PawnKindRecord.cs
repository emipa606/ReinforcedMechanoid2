using System.Xml;
using Verse;

namespace ReinforcedMechanoids;

public class PawnKindRecord
{
    public float commonality;
    public PawnKindDef pawnKind;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "pawnKind", xmlRoot);
        commonality = ParseHelper.FromString<float>(xmlRoot.FirstChild.Value);
    }
}