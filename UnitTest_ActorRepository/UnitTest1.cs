using Microsoft.EntityFrameworkCore;
using REST_ActorReporsitory.Models;
using REST_ActorReporsitory.Repository;

namespace UnitTest_ActorRepository
{
    public class ActorRepositoryTest
    {
        private bool useDatabase = false;
        private IActorRepository repo;

        public ActorRepositoryTest()
        {
            if (useDatabase)
            {
                var optionsBuilder = new DbContextOptionsBuilder<ActorDbContext>();
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Cat_register;Trusted_Connection=true");
                ActorDbContext context = new(optionsBuilder.Options);
                context.Database.ExecuteSqlRaw("DELETE FROM dbo.Actors");
                repo = new ActorRepositoryDataBase(context);
            }
            else
            {
                repo = new ActorRepositoryList();
            }
        }

        [Fact]
        public void Add_Test()
        {
            Actor added = repo.Add(new Actor { Name = "Tom Hanks", birthYear = 1956 });

            Assert.True(added.Id > 0);
            Assert.Equal("Tom Hanks", added.Name);
            Assert.Equal(1956, added.birthYear);
            Assert.Equal(1, repo.GetAll().Count());
        }

        [Fact]
        public void GetAll_Test()
        {
            repo.Add(new Actor { Name = "Actor1", birthYear = 1990 });
            repo.Add(new Actor { Name = "Actor2", birthYear = 1985 });

            Assert.Equal(2, repo.GetAll().Count());
        }

        [Fact]
        public void GetById_Test()
        {
            Actor added = repo.Add(new Actor { Name = "Brad Pitt", birthYear = 1963 });

            Assert.NotNull(repo.GetById(added.Id));
            Assert.Null(repo.GetById(999));
        }

        [Fact]
        public void Delete_Test()
        {
            Actor added = repo.Add(new Actor { Name = "ToDelete", birthYear = 2000 });

            Assert.NotNull(repo.Delete(added.Id));
            Assert.Null(repo.GetById(added.Id));
            Assert.Null(repo.Delete(999));
        }

        [Fact]
        public void Update_Test()
        {
            Actor added = repo.Add(new Actor { Name = "Old Name", birthYear = 1980 });

            Actor? updated = repo.Update(added.Id, new Actor { Name = "New Name", birthYear = 2000 });

            Assert.NotNull(updated);
            Assert.Equal("New Name", updated!.Name);
            Assert.Equal(2000, updated.birthYear);
            Assert.Null(repo.Update(999, new Actor { Name = "X", birthYear = 1 }));
        }

        [Fact]
        public void GetAll_FilterByName_Test()
        {
            repo.Add(new Actor { Name = "Tom Hanks", birthYear = 1956 });
            repo.Add(new Actor { Name = "Tom Cruise", birthYear = 1962 });
            repo.Add(new Actor { Name = "Brad Pitt", birthYear = 1963 });

            var result = repo.GetAll(nameFilter: "Tom").ToList();
            Assert.Equal(2, result.Count);
            Assert.All(result, a => Assert.Contains("Tom", a.Name!));

            var noMatch = repo.GetAll(nameFilter: "XYZ").ToList();
            Assert.Empty(noMatch);
        }

        [Fact]
        public void GetAll_SortByName_Test()
        {
            repo.Add(new Actor { Name = "Charlie", birthYear = 1990 });
            repo.Add(new Actor { Name = "Alice", birthYear = 1985 });
            repo.Add(new Actor { Name = "Bob", birthYear = 1980 });

            var asc = repo.GetAll(sortBy: "name").ToList();
            Assert.Equal("Alice", asc[0].Name);
            Assert.Equal("Bob", asc[1].Name);
            Assert.Equal("Charlie", asc[2].Name);

            var desc = repo.GetAll(sortBy: "name_desc").ToList();
            Assert.Equal("Charlie", desc[0].Name);
            Assert.Equal("Bob", desc[1].Name);
            Assert.Equal("Alice", desc[2].Name);
        }

        [Fact]
        public void GetAll_SortByBirthYear_Test()
        {
            repo.Add(new Actor { Name = "Young", birthYear = 2000 });
            repo.Add(new Actor { Name = "Old", birthYear = 1950 });
            repo.Add(new Actor { Name = "Middle", birthYear = 1975 });

            var asc = repo.GetAll(sortBy: "birthYear").ToList();
            Assert.Equal(1950, asc[0].birthYear);
            Assert.Equal(1975, asc[1].birthYear);
            Assert.Equal(2000, asc[2].birthYear);

            var desc = repo.GetAll(sortBy: "birthYear_desc").ToList();
            Assert.Equal(2000, desc[0].birthYear);
            Assert.Equal(1975, desc[1].birthYear);
            Assert.Equal(1950, desc[2].birthYear);
        }

        [Fact]
        public void GetAll_FilterAndSort_Test()
        {
            repo.Add(new Actor { Name = "Tom Hanks", birthYear = 1956 });
            repo.Add(new Actor { Name = "Tom Cruise", birthYear = 1962 });
            repo.Add(new Actor { Name = "Brad Pitt", birthYear = 1963 });

            var result = repo.GetAll(nameFilter: "Tom", sortBy: "birthYear").ToList();
            Assert.Equal(2, result.Count);
            Assert.Equal("Tom Hanks", result[0].Name);
            Assert.Equal("Tom Cruise", result[1].Name);
        }
    }
}
