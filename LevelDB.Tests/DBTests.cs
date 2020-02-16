namespace LevelDB.Tests
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Xunit;

    public class DBTests
    {
        [Fact]
        public void GetIterable()
        {
            using (LevelDB.DB<string, string> db = GetTestDb())
            {
                Assert.Equal(
                    "a=1;b=2;c=3;l=12;m=13;n=14;x=24;y=25;z=26",
                    string.Join(";", db.GetIterable().Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void Get()
        {
            using (LevelDB.DB<string, string> db = GetTestDb())
            {
                Assert.Equal("1", db.Get("a"));
                Assert.Equal("26", db.Get("z"));
                Assert.Equal("1", db["a"]);
                Assert.Equal("26", db["z"]);
            }
        }

        [Fact]
        public void GetIterableReverse()
        {
            using (LevelDB.DB<string, string> db = GetTestDb())
            {
                Assert.Equal(
                    "z=26;y=25;x=24;n=14;m=13;l=12;c=3;b=2;a=1",
                    string.Join(";", db.GetIterable().Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void GetIterableRange()
        {
            using (LevelDB.DB<string, string> db = GetTestDb())
            {
                Assert.Equal(
                    "c=3;l=12;m=13;n=14;x=24",
                    string.Join(";", db.GetIterable().Range("c", "y").Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void GetIterableReverseRange()
        {
            using (LevelDB.DB<string, string> db = GetTestDb())
            {
                Assert.Equal(
                    "y=25;x=24;n=14;m=13;l=12",
                    string.Join(";", db.GetIterable().Reverse().Range("y", "c").Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void GetIterableRangeReverse()
        {
            using (LevelDB.DB<string, string> db = GetTestDb())
            {
                Assert.Equal(
                    "y=25;x=24;n=14;m=13;l=12",
                    string.Join(";", db.GetIterable().Range("c", "y").Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void Integers()
        {
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using (var db = new LevelDB.DB(options, $"/tmp/DBTests-Integers").Cast<int, string>())
            {
                db.Put(1, "aaa").Put(2, "bbb").Put(3, "ccc");
                db.Put(7, "ggg").Put(8, "hhh").Put(9, "iii");
                db.Put(6, "fff").Put(5, "eee").Put(4, "ddd");
                Assert.Equal(
                    "4=ddd;5=eee;6=fff;7=ggg",
                    string.Join(";", db.GetIterable().Range(4, 8).Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void Join()
        {
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            using (var db = new LevelDB.DB(options, $"/tmp/DBTests-Join"))
            {
                var parents = db.Cast<Parent, string>();
                var children = db.Cast<Child, string>();
                parents.Put(new Parent(1), "aaa");
                parents.Put(new Parent(2), "bbb");
                children.Put(new Child(1, 1), "afirst");
                children.Put(new Child(1, 2), "asecond");
                children.Put(new Child(2, 1), "afirst");
                children.Put(new Child(2, 2), "asecond");
                var join =
                    from parent
                        in parents.GetIterable().Range(new Parent(1), new Parent(3))
                    join child
                        in children.GetIterable().Range(new Child(1, 1), new Child(3, 1))
                        on parent.Key.parentId equals child.Key.parentId
                    select $"[{parent.Value} => {child.Key.parentId}:{child.Key.childId}:{child.Value}]";
                Assert.Equal(
                    "[aaa => 1:1:afirst]; [aaa => 1:2:asecond]; [bbb => 2:1:afirst]; [bbb => 2:2:asecond]",
                    string.Join("; ", join));
            }
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
    }
}