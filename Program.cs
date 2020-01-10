namespace Triangle
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello World! version={LevelDB.Native.leveldb_major_version()}");

            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using (LevelDB.DB db = new LevelDB.DB(options, "/tmp/testdb"))
            {
                db.Put("z", "9").Put("m", "5").Put("b", "1");
                for (var it = db.GetIterator().Seek("c"); it.IsValid; it.Next())
                {
                    Console.WriteLine($"key = {it.Current.Key} => value = {it.Current.Value}");
                }
            }
        }
    }
}
