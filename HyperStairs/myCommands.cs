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

        [CommandMethod("CB")]
        public void CreateBlock()
        {
            Document doc =
                Application.DocumentManager.MdiActiveDocument;
            Database db = doc.Database;
            Editor ed = doc.Editor;

            Transaction tr =
                db.TransactionManager.StartTransaction();
            using (tr)
            {
                // Get the block table from the drawing

                BlockTable bt =
                    (BlockTable) tr.GetObject(
                        db.BlockTableId,
                        OpenMode.ForRead
                        );

                // Check the block name, to see whether it's
                // already in use

                PromptStringOptions pso = new PromptStringOptions("\nEnter new block name: ");
                pso.AllowSpaces = true;

                // A variable for the block's name

                string blkName = "";

                do
                {
                    PromptResult pr = ed.GetString(pso);

                    // Just return if the user cancelled
                    // (will abort the transaction as we drop out of the using
                    // statement's scope)

                    if (pr.Status != PromptStatus.OK)
                        return;

                    try
                    {
                        // Validate the provided symbol table name

                        SymbolUtilityServices.ValidateSymbolName(
                            pr.StringResult,
                            false
                            );

                        // Only set the block name if it isn't in use

                        if (bt.Has(pr.StringResult))
                            ed.WriteMessage("\nA block with this name already exists.");
                        else
                            blkName = pr.StringResult;
                    }
                    catch
                    {
                        // An exception has been thrown, indicating the
                        // name is invalid

                        ed.WriteMessage("\nInvalid block name.");
                    }
                } while (blkName == "");

                // Create our new block table record...

                BlockTableRecord btr = new BlockTableRecord();

                // ... and set its properties

                btr.Name = blkName;

                // Add the new block to the block table

                bt.UpgradeOpen();
                ObjectId btrId = bt.Add(btr);
                tr.AddNewlyCreatedDBObject(btr, true);

                // Add some lines to the block to form a square
                // (the entities belong directly to the block)

                DBObjectCollection ents = SquareOfLines(5);
                foreach (Entity ent in ents)
                {
                    btr.AppendEntity(ent);
                    tr.AddNewlyCreatedDBObject(ent, true);
                }

                // Add a block reference to the model space

                BlockTableRecord ms =
                    (BlockTableRecord) tr.GetObject(
                        bt[BlockTableRecord.ModelSpace],
                        OpenMode.ForWrite
                        );

                BlockReference br =
                    new BlockReference(Point3d.Origin, btrId);

                ms.AppendEntity(br);
                tr.AddNewlyCreatedDBObject(br, true);

                // Commit the transaction

                tr.Commit();

                // Report what we've done

                ed.WriteMessage(
                    "\nCreated block named \"{0}\" containing {1} entities.",
                    blkName, ents.Count
                    );
            }
        }

        private DBObjectCollection SquareOfLines(double size)
        {
            // A function to generate a set of entities for our block

            DBObjectCollection ents = new DBObjectCollection();
            Point3d[] pts =
            {
                new Point3d(-size, -size, 0),
                new Point3d(size, -size, 0),
                new Point3d(size, size, 0),
                new Point3d(-size, size, 0)
            };
            int max = pts.GetUpperBound(0);

            for (int i = 0; i <= max; i++)
            {
                int j = (i == max ? 0 : i + 1);
                Line ln = new Line(pts[i], pts[j]);
                ents.Add(ln);
            }
            return ents;
        }
    }
}