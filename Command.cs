#region Namespaces
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;

#endregion

namespace Session_04_Challange
{
    [Transaction(TransactionMode.Manual)]
    public class Command : IExternalCommand
    {
        public Result Execute(
          ExternalCommandData commandData,
          ref string message,
          ElementSet elements)
        {
            UIApplication uiapp = commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Application app = uiapp.Application;
            Document doc = uidoc.Document;


            TaskDialog.Show("APP", "select lines");
            //setup list of lines
            IList<Element> PickList = uidoc.Selection.PickElementsByRectangle("select lines");
            List<CurveElement> LineList = new List<CurveElement>();
            
            foreach (Element element in PickList)
            {
                if(element is CurveElement)
                {
                    CurveElement curve = element as CurveElement;
                   
                    if(curve.CurveElementType == CurveElementType.ModelCurve)
                     LineList.Add(curve);
                    
                }
            }
            
            
            //create items based on line type
            Transaction t = new Transaction(doc);
            t.Start("Create elements");
            Level NewLevel = GetLevelByName(doc, "Level 2");
            //Level NewLevel = Level.Create(doc, 10);
            WallType WallT = GetWallTypeByName(doc, "Generic - 8\"");
            WallType StoreFrontT = GetWallTypeByName(doc, "Storefront");
            Double wallH = 10;
            
            MEPSystemType pipeSystemT = GetMEPSystemTypeByName(doc, "Domestic Hot Water");
            PipeType pipeT = GetPipeTypeByName(doc, "Default");
           
            MEPSystemType ductSystemT = GetMEPSystemTypeByName(doc, "Supply Air");
            DuctType ductT = GetDuctTypeByName(doc, "Default");

            foreach (CurveElement CurrentCurve in LineList)
            {
                GraphicsStyle GS = CurrentCurve.LineStyle as GraphicsStyle;
                Curve curve = CurrentCurve.GeometryCurve;
                //XYZ StartPoint = curve.GetEndPoint(0);
                //XYZ EndPoint = curve.GetEndPoint(1);
                

                switch (GS.Name)
                {
                    case "A-GLAZ":
                        Wall STWall = Wall.Create(doc, curve, StoreFrontT.Id, NewLevel.Id, wallH, 0, false, false); 
                        break;

                    case "A-WALL":
                        Wall Nwall = Wall.Create(doc, curve, WallT.Id, NewLevel.Id, wallH, 0, false, false);
                        break;

                    case "M-DUCT":
                        Duct NewDuct = Duct.Create(doc, ductSystemT.Id, ductT.Id, NewLevel.Id, curve.GetEndPoint(0), curve.GetEndPoint(1));
                        
                        break;

                    case "P-PIPE":
                        Pipe NPipe =Pipe.Create(doc,pipeSystemT.Id,pipeT.Id,NewLevel.Id,curve.GetEndPoint(0), curve.GetEndPoint(1));
                        
                        break;
                       
                    default:
                        
                        break;
                }
            }

            






            t.Commit();
            t.Dispose();
            return Result.Succeeded;
        }

        private Level GetLevelByName(Document doc, string LevelName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Level));

            foreach (Level cLevelName in collector)
            {
                if (cLevelName.Name == LevelName)
                    return cLevelName;

            }
            return null;
        }

        private DuctType GetDuctTypeByName(Document doc, string DuctType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(DuctType));

            foreach (DuctType CDuctType in collector.Cast<DuctType>())
            {
                if (CDuctType.Name == DuctType)
                    return CDuctType;

            }
            return null;

        }

        private PipeType GetPipeTypeByName(Document doc, string PipeType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(PipeType));

            foreach (PipeType CPipeType in collector.Cast<PipeType>())
            {
                if (CPipeType.Name == PipeType)
                    return CPipeType;

            }
            return null;
        }

        private MEPSystemType GetMEPSystemTypeByName(Document doc, string MepSystemType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(MEPSystemType));

            foreach (MEPSystemType CMepSystemType in collector.Cast<MEPSystemType>())
            {
                if (CMepSystemType.Name == MepSystemType)
                    return CMepSystemType;

            }
            return null;
        }

        private WallType GetWallTypeByName(Document doc, string WallType)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(WallType));
            
            foreach(WallType CwallType in collector)
            {
                if (CwallType.Name == WallType)
                    return CwallType;

            }
            return null;
        }
    }
}
