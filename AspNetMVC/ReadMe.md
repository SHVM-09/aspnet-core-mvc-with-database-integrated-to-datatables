## Create Razor Pages Web App 

```
mkdir RazorPagesMovie // make
cd RazorPagesMovie // change dir
dotnet new webapp -o RazorPagesMovie // start
code -r RazorPagesMovie // instance
dotnet dev-certs https --trust // trust certificates
dotnet build // build (development)
dotnet watch // serve ( with auto-refresh )
dotnet watch --no-hot-reload ( watch without hot-reload - manual reload required)
dotnet run // serve one time ( does not support hot-reload )
dotnet publish // build (production)
dotnet --version // check version 
dotnet new // gives types of project ex - MVC, console, Razor etc ...
dotnet --list-sdks // list all sdks --versions in system
```

<br/>
<hr/>
All steps till here will form a basic structure for the .NET project
<hr/>
<br/>
Go through link below for AspNet C# syntaxes:

https://gist.github.com/jwill9999/655533b6652418bd3bc94d864a5e2b49

<hr/>

## Add Data Model

make folder _Model_ in RazorPagesMovie
add _Movie.cs_ class file in Model folder (refer below)

<hr/>

```
using System.ComponentModel.DataAnnotations;

namespace RazorPagesMovie.Models;

public class Movie
{
    public int Id { get; set; }
    public string? Title { get; set; }
    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public decimal Price { get; set; }
}

```

<hr/>

## Add NuGet packages and EF tools

```
dotnet tool uninstall --global dotnet-aspnet-codegenerator
dotnet tool install --global dotnet-aspnet-codegenerator 
dotnet tool uninstall --global dotnet-ef
dotnet tool install --global dotnet-ef
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.SQLite
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
dotnet add package Microsoft.EntityFrameworkCore.Tools
```
## Scaffold the movie model

```
dotnet aspnet-codegenerator razorpage -m Movie -dc RazorPagesMovie.Data.RazorPagesMovieContext -udl -outDir Pages/Movies --referenceScriptLibraries --databaseProvider sqlite

-m	=> The name of the model.
-dc	=> The DbContext class to use including namespace.
-udl	=> Use the default layout.
-outDir => The relative output folder path to create the views.
--referenceScriptLibraries =>	Adds _ValidationScriptsPartial to Edit and Create pages

```

The scaffolding tool produces pages for Create, Read, Update, and Delete (CRUD) operations for the movie model

```
dotnet aspnet-codegenerator razorpage -h // for help
```

<hr/>

## Create the initial database schema using EF's migration feature

```
dotnet ef migrations add InitialCreate
dotnet ef database update
```

<hr/>

## The Create, Delete, Details, and Edit pages

## DB Browser for SQLite

[SQLite Docs-download](https://sqlitebrowser.org/)

<hr/>

## Seed the Database

```
using Microsoft.EntityFrameworkCore;
using RazorPagesMovie.Data;

namespace RazorPagesMovie.Models;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new RazorPagesMovieContext(
            serviceProvider.GetRequiredService<
                DbContextOptions<RazorPagesMovieContext>>()))
        {
            if (context == null || context.Movie == null)
            {
                throw new ArgumentNullException("Null RazorPagesMovieContext");
            }

            // Look for any movies.
            if (context.Movie.Any())
            {
                return;   // DB has been seeded
            }

            context.Movie.AddRange(
                new Movie
                {
                    Title = "When Harry Met Sally",
                    ReleaseDate = DateTime.Parse("1989-2-12"),
                    Genre = "Romantic Comedy",
                    Price = 7.99M
                },

                new Movie
                {
                    Title = "Ghostbusters ",
                    ReleaseDate = DateTime.Parse("1984-3-13"),
                    Genre = "Comedy",
                    Price = 8.99M
                },

                new Movie
                {
                    Title = "Ghostbusters 2",
                    ReleaseDate = DateTime.Parse("1986-2-23"),
                    Genre = "Comedy",
                    Price = 9.99M
                },

                new Movie
                {
                    Title = "Rio Bravo",
                    ReleaseDate = DateTime.Parse("1959-4-15"),
                    Genre = "Western",
                    Price = 3.99M
                }
            );
            context.SaveChanges();
        }
    }
}
```

<hr/>

## Add Search

```
  [BindProperty(SupportsGet = true)]
    public string? SearchString { get; set; }

    public SelectList? Genres { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? MovieGenre { get; set; }

// update OnGetAsync

    public async Task OnGetAsync()
    {
        var movies = from m in _context.Movie
                     select m;
        if (!string.IsNullOrEmpty(SearchString))
        {
            movies = movies.Where(s => s.Title.Contains(SearchString));
        }

        Movie = await movies.ToListAsync();
    }

```

Add this in index.cshtml

```
<form>
    <select asp-for="MovieGenre" asp-items="Model.Genres">
        <option value="">All</option>
    </select>
    <p>
        Title: <input type="text" asp-for="SearchString" />
        <input type="submit" value="Filter" />
    </p>
</form>
```

For search by Genre

```

    public async Task OnGetAsync()
    {
        // Use LINQ to get list of genres.
        IQueryable<string> genreQuery = from m in _context.Movie
                                        orderby m.Genre
                                        select m.Genre;

        var movies = from m in _context.Movie
                     select m;

        if (!string.IsNullOrEmpty(SearchString))
        {
            movies = movies.Where(s => s.Title.Contains(SearchString));
        }

        if (!string.IsNullOrEmpty(MovieGenre))
        {
            movies = movies.Where(x => x.Genre == MovieGenre);
        }
        Genres = new SelectList(await genreQuery.Distinct().ToListAsync());
        Movie = await movies.ToListAsync();
    }
```
<hr/>

## Add New Field + Validation

Add this in Models/Movie.cs

```

public string Rating { get; set; } = string.Empty;

// Add this to Pages/Models/Index.cshtml

    <th>
        @Html.DisplayNameFor(model => model.Movie[0].Rating)
    </th>

// &

    <td>
        @Html.DisplayFor(modelItem => item.Rating)
    </td>


```

<hr/>

Update the SeedData class so that it provides a value for the new column. A sample change is shown below, but make this change for each new Movie block

## Add a migration for rating

```
dotnet ef migrations add rating
dotnet ef database update
```
### change this @ Movie class

```
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RazorPagesMovie.Models;

public class Movie
{
    public int Id { get; set; }

    [StringLength(60, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Release Date"), DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; }

    [RegularExpression(@"^[A-Z]+[a-zA-Z\s]*$"), Required, StringLength(30)]
    public string Genre { get; set; } = string.Empty;

    [Range(1, 100), DataType(DataType.Currency)]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Price { get; set; }

    [RegularExpression(@"^[A-Z]+[a-zA-Z0-9""'\s-]*$"), StringLength(5)]
    public string Rating { get; set; } = string.Empty;
}
```


