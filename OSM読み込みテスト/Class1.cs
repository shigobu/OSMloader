using System;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD;


namespace OSM読み込みテスト
{
    public class Class1
    {
        /// <summary>
        /// osmファイルを読み込む
        /// </summary>
        [CommandMethod("OsmLoad")]
        public void OsmLoad()
        {
            Database Db = HostApplicationServices.WorkingDatabase;
            string FileName = null;
            using (var ofd = new OpenFileDialog())
            {
                ofd.InitialDirectory = @"C:\";
                ofd.Filter = "OSMファイル(*.osm)|*.osm|すべてのファイル(*.*)|*.*";
                DialogResult result = ofd.ShowDialog();
                //キャンセルの場合、終了
                if (result == DialogResult.OK)
                {
                    FileName = ofd.FileName;
                }
                else
                {
                    return;
                }
            }

            Transaction Tr = Db.TransactionManager.StartTransaction();
            try
            {
                var Bt = (BlockTable)(Tr.GetObject(Db.BlockTableId, OpenMode.ForRead));
                var Btr = (BlockTableRecord)(Tr.GetObject(Bt["*MODEL_SPACE"], OpenMode.ForWrite));

                //xml読み込みクラス
                var xml = XDocument.Load(FileName);
                XElement osm = xml.Element("osm");
                //ノードを保持する。
                var nodes = new Dictionary<long, Point2d>();
                foreach (var node in osm.Elements("node"))
                {
                    double lon = double.Parse(node.Attribute("lon").Value);
                    double lat = double.Parse(node.Attribute("lat").Value);
                    double x, y;
                    cXY.LonLat2XY(lon, lat, 35.833333d, 141d, out x, out y);
                    nodes.Add(long.Parse(node.Attribute("id").Value), new Point2d(lon, lat));
                }
                foreach (var way in osm.Elements("way"))
                {
                    Polyline poly = new Polyline();
                    //poly.Layer = "0";
                    foreach (var nd in way.Elements("nd"))
                    {
                        poly.AddVertexAt(poly.NumberOfVertices, nodes[long.Parse(nd.Attribute("ref").Value)], 0, 0, 0);
                    }
                    Btr.AppendEntity(poly);
                    Tr.AddNewlyCreatedDBObject(poly, true);

                }
                Tr.Commit();
            }
            catch(System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                Tr.Dispose();
            }


        }
    }
}
