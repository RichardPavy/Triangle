namespace Triangle
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Hello World! version={LevelDB.Native.leveldb_major_version()}.{LevelDB.Native.leveldb_minor_version()}");

            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using (LevelDB.DB db = new LevelDB.DB(options, "/tmp/testdb"))
            {
                db.Put("z", "26").Put("m", "13").Put("b", "2");
                db.Put("c", "3").Put("s", "19").Put("t", "20").Put("u", "21");
                foreach (var kv in db.GetIterable().Range("c", "t").Reverse())
                {
                    Console.WriteLine($"key = {kv.Key} => value = {kv.Value}");
                }
                Console.WriteLine("Done!");
                foreach (var kv in db.GetIterable().Reverse().Range("c", "t"))
                {
                    Console.WriteLine($"key = {kv.Key} => value = {kv.Value}");
                }
                Console.WriteLine("Done!");
            }
        }
    }
}
