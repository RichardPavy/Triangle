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
            using (LevelDB.DB db = GetTestDb())
            {
                Assert.Equal(
                    "a=1;b=2;c=3;l=12;m=13;n=14;x=24;y=25;z=26",
                    string.Join(";", db.GetIterable().Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void Get()
        {
            using (LevelDB.DB db = GetTestDb())
            {
                Assert.Equal("1", db.Get("a"));
                Assert.Equal("26",db.Get("z"));
                Assert.Equal("1", db["a"]);
                Assert.Equal("26", db["z"]);
            }
        }

        [Fact]
        public void GetIterableReverse()
        {
            using (LevelDB.DB db = GetTestDb())
            {
                Assert.Equal(
                    "z=26;y=25;x=24;n=14;m=13;l=12;c=3;b=2;a=1",
                    string.Join(";", db.GetIterable().Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void GetIterableRange()
        {
            using (LevelDB.DB db = GetTestDb())
            {
                Assert.Equal(
                    "c=3;l=12;m=13;n=14;x=24",
                    string.Join(";", db.GetIterable().Range("c", "y").Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void GetIterableReverseRange()
        {
            using (LevelDB.DB db = GetTestDb())
            {
                Assert.Equal(
                    "y=25;x=24;n=14;m=13;l=12",
                    string.Join(";", db.GetIterable().Reverse().Range("y", "c").Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        [Fact]
        public void GetIterableRangeReverse()
        {
            using (LevelDB.DB db = GetTestDb())
            {
                Assert.Equal(
                    "y=25;x=24;n=14;m=13;l=12",
                    string.Join(";", db.GetIterable().Range("c", "y").Reverse().Select(kv => $"{kv.Key}={kv.Value}")));
            }
        }

        private LevelDB.DB GetTestDb([CallerMemberName] string name = null)
        {
            LevelDB.Options options = new LevelDB.Options();
            options.CreateIfMissing = true;
            var db = new LevelDB.DB(options, $"/tmp/{name}");
            db.Put("c", "3").Put("b", "2").Put("a", "1");
            db.Put("l", "12").Put("m", "13").Put("n", "14");
            db.Put("z", "26").Put("y", "25").Put("x", "24");
            return db;
        }
    }
}
