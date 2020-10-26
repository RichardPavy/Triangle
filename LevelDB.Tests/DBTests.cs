namespace Triangle.LevelDB.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Triangle.LevelDB.Iterables;
    using Xunit;

    // DYLD_INSERT_LIBRARIES=/usr/local/lib/libtcmalloc.dylib dotnet test LevelDB.Tests
    public class DBTests
    {
        [Fact]
        public void GetIterable()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal(
                "a=1;b=2;c=3;l=12;m=13;n=14;x=24;y=25;z=26",
                string.Join(";", db.GetIterable().Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void Get()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal("1", db.Get("a"));
            Assert.Equal("26", db.Get("z"));
            Assert.Equal("1", db["a"]);
            Assert.Equal("26", db["z"]);
        }

        [Fact]
        public void GetIterableReverse()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal(
                "z=26;y=25;x=24;n=14;m=13;l=12;c=3;b=2;a=1",
                string.Join(";", db.GetIterable().Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void GetIterableRange()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal(
                "c=3;l=12;m=13;n=14;x=24",
                string.Join(";", db.GetIterable().Range("c", "y").Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void GetIterableReverseRange()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal(
                "y=25;x=24;n=14;m=13;l=12",
                string.Join(";", db.GetIterable().Reverse().Range("y", "c").Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void GetIterableRangeReverse()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal(
                "y=25;x=24;n=14;m=13;l=12",
                string.Join(";", db.GetIterable().Range("c", "y").Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void Integers()
        {
            using var log = Log();
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using LevelDB.DB<int, string> db = new LevelDB.DB(options, $"/tmp/DBTests-Integers").Cast<int, string>();
            db.Put(1, "aaa").Put(2, "bbb").Put(3, "ccc");
            db.Put(7, "ggg").Put(8, "hhh").Put(9, "iii");
            db.Put(6, "fff").Put(5, "eee").Put(4, "ddd");
            Assert.Equal(
                "4=ddd;5=eee;6=fff;7=ggg",
                string.Join(";", db.GetIterable().Range(4, 8).Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void Join()
        {
            using var log = Log();
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using LevelDB.DB db = new LevelDB.DB(options, $"/tmp/DBTests-Join");
            var parents = db.Cast<Parent, string>();
            var children = db.Cast<Child, string>();
            parents.Put(new Parent(1), "aaa");
            parents.Put(new Parent(2), "bbb");
            children.Put(new Child(1, 11), "afirst");
            children.Put(new Child(1, 12), "asecond");
            children.Put(new Child(2, 21), "afirst");
            children.Put(new Child(2, 22), "asecond");
            var join =
                from parent
                    in parents.GetIterable().Range(new Parent(1), new Parent(3))
                join child
                    in children.GetIterable().Range(new Child(1, 1), new Child(3, 1))
                    on parent.Key.parentId equals child.Key.parentId
                select $"[{parent.Value} => {child.Key.parentId}:{child.Key.childId}:{child.Value}]";
            Assert.Equal(
                "[aaa => 1:11:afirst]; [aaa => 1:12:asecond]; [bbb => 2:21:afirst]; [bbb => 2:22:asecond]",
                string.Join("; ", join));
            IEnumerable<KeyValuePair<Tuple<Parent, Child>, Tuple<string, string>>> streamJoin =
                parents.GetIterable().Range(new Parent(1), new Parent(3))
                    .Join(
                        children.GetIterable().Range(new Child(1, 1), new Child(3, 1)),
                        (k1, k2) => k1.parentId.CompareTo(k2.parentId));
            Assert.Equal(
                "1/11 => aaa/afirst; 1/12 => aaa/asecond; 2/21 => bbb/afirst; 2/22 => bbb/asecond",
                string.Join("; ", streamJoin.Select(row => $"{row.Key.Item1.parentId}/{row.Key.Item2.childId} => {row.Value.Item1}/{row.Value.Item2}")));
        }

        struct Parent
        {
            private readonly int table;
            internal readonly int parentId;

            internal Parent(int parentId)
            {
                this.table = 1;
                this.parentId = parentId;
            }

            public override string ToString()
            {
                return $"[Parent] table:{table} parentId{parentId}";
            }
        }

        struct Child
        {
            private readonly int table;
            internal readonly int parentId;
            internal readonly int childId;

            internal Child(int parentId, int childId)
            {
                this.table = 2;
                this.parentId = parentId;
                this.childId = childId;
            }

            public override string ToString()
            {
                return $"[Child] table:{table} parentId{parentId} childId:{childId}";
            }
        }

        [Fact]
        public void Delete()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            db.Delete("a").Delete("c").Delete("m").Delete("n");
            Assert.Equal(
                "b=2;l=12;x=24;y=25;z=26",
                string.Join(";", db.GetIterable().Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void Snapshot()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            IIterable<string, string> it = db.GetIterable().Snapshot();
            db.Delete("a").Delete("c").Delete("m").Delete("n");
            Assert.Equal(
                "a=1;b=2;c=3;l=12;m=13;n=14;x=24;y=25;z=26",
                string.Join(";", it.Select(kv => $"{kv.Key}={kv.Value}")));
            it.Snapshot();
            Assert.Equal(
                "b=2;l=12;x=24;y=25;z=26",
                string.Join(";", it.Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void Infinity()
        {
            using var log = Log();
            using LevelDB.DB<string, string> db = GetTestDb();
            Assert.Equal(
                "y=25;z=26",
                string.Join(";", db.GetIterable().Range("y", null).Select(kv => $"{kv.Key}={kv.Value}")));
            Assert.Equal(
                "z=26;y=25",
                string.Join(";", db.GetIterable().Reverse().Range(null, "x").Select(kv => $"{kv.Key}={kv.Value}")));
        }

        [Fact]
        public void Prefix()
        {
            using var log = Log();
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using var db = new LevelDB.DB(options, $"/tmp/DBTests-Prefix").Cast<string, int>();
            db
                .Put("a", 1)
                .Put("aa", 2)
                .Put("ab", 3)
                .Put("b", 4)
                .Put("ba", 5)
                .Put("bb", 6)
                .Put("c", 7);
            Assert.Equal(
                "a=1;aa=2;ab=3",
                string.Join(";", db.GetIterable().Prefix("a").Select(kv => $"{kv.Key}={kv.Value}")));
            Assert.Equal(
                "b=4;ab=3;aa=2",
                string.Join(";", db.GetIterable().Reverse().Prefix("a").Select(kv => $"{kv.Key}={kv.Value}")));
            Assert.Equal(
                "b=4;ab=3;aa=2",
                string.Join(";", db.GetIterable().Prefix("a").Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
        }

        private LevelDB.DB<string, string> GetTestDb([CallerMemberName] string name = null)
        {
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            var db = new LevelDB.DB(options, $"/tmp/DBTests-{name}").Cast<string, string>();
            db.Put("c", "3").Put("b", "2").Put("a", "1");
            db.Put("l", "12").Put("m", "13").Put("n", "14");
            db.Put("z", "26").Put("y", "25").Put("x", "24");
            return db;
        }

        private static IDisposable Log(string message = "", [CallerMemberName] string callerMemberName = null)
        {
            return new LogScope(string.IsNullOrEmpty(message) ? callerMemberName : $"{callerMemberName}: {message}");
        }

        private class LogScope : IDisposable
        {
            private readonly string log;
            private readonly Stopwatch stopwatch = Stopwatch.StartNew(); 

            internal LogScope(string log)
            {
                this.log = log;
                Console.WriteLine(log + ": Start");
            }

            public void Dispose()
            {
                Console.WriteLine($"{log}: {stopwatch.Elapsed}");
            }
        }
    }
}
