using System;
using System.Windows;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Application = Autodesk.AutoCAD.ApplicationServices.Application;

// This line is not mandatory, but improves loading performances

[assembly: CommandClass(typeof(HyperStairs.MyCommands))]

namespace HyperStairs
{
    // This class is instantiated by AutoCAD for each document when
    // a command is called by the user the first time in the context
    // of a given document. In other words, non static data in this class
    // is implicitly per-document!
    public class MyCommands
    {
        // The CommandMethod attribute can be applied to any public  member 
        // function of any public class.
        // The function should take no arguments and return nothing.
        // If the method is an intance member then the enclosing class is 
        // intantiated for each document. If the member is a static member then
        // the enclosing class is NOT intantiated.
        //
        // NOTE: CommandMethod has overloads where you can provide helpid and
        // context menu.

        // Modal Command with localized name
        /*
        [CommandMethod("MyGroup", "MyCommand", "MyCommandLocal", CommandFlags.Modal)]
        public void MyCommand() // This method can have any name
        {
            // Put your command code here
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed;
            if (doc != null)
            {
                ed = doc.Editor;
                PromptResult pr = ed.GetString("Enter string, please");
                ed.WriteMessage("Hello, this is your first command with argument: " + pr.StringResult);
            }
        }
        */

        [CommandMethod("CreateStairs")]
        public static void CreateStairs()
        {
            // Получение текущего документа и базы данных
            Document acDoc = Application.DocumentManager.MdiActiveDocument;
            Database acCurDb = acDoc.Database;

            // Старт транзакции
            using (Transaction tr = acCurDb.TransactionManager.StartTransaction())
            {
                Document doc = Application.DocumentManager.MdiActiveDocument;
                Editor ed;

                ed = doc.Editor;
                double l = ed.GetDouble("Print L").Value;
                double h = ed.GetDouble("Print H").Value;
                double w = ed.GetDouble("Print W").Value;

                // Открытие таблицы Блоков для чтения
                BlockTable bt;
                bt = tr.GetObject(acCurDb.BlockTableId, OpenMode.ForRead) as BlockTable;

                // Открытие записи таблицы Блоков пространства Модели для записи
                BlockTableRecord btr;
                btr = tr.GetObject(bt[BlockTableRecord.ModelSpace],
                    OpenMode.ForWrite) as BlockTableRecord;

                // Создание отрезка начинающегося в 5,5 и заканчивающегося в 12,3
                AddLine(new Point3d(0, 0, 0), new Point3d(0, l, h), tr, btr);
                AddLine(new Point3d(w, 0, 0), new Point3d(w, l, h), tr, btr);
                AddLine(new Point3d(0, 0, 0), new Point3d(w, 0, 0), tr, btr);
                AddLine(new Point3d(0, l, h), new Point3d(w, l, h), tr, btr);

                // Сохранение нового объекта в базе данных
                tr.Commit();
            }
        }

        public static void AddLine(Point3d p1, Point3d p2, Transaction t, BlockTableRecord r)
        {
            Line l = new Line(p1, p2);
            l.SetDatabaseDefaults();
            r.AppendEntity(l);
            t.AddNewlyCreatedDBObject(l, true);
        }
    }
}