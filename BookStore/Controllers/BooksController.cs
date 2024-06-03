using BookStore.Data;
using BookStore.Models;
using BookStore.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BookStore.Controllers
{
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext context;

        public BooksController(ApplicationDbContext context)
        {
            this.context = context;
        }

        public IActionResult Index()
        {
            var books = context.Books
                .Include(book => book.Author)
                .Include(book => book.Categories)
                .ThenInclude(book => book.category)
                .ToList();

            var bookVms = new List<BookVM>();
            foreach (var book in books)
            {
                var bookVM = new BookVM
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author.Name,
                    Publisher = book.Publisher,
                    publishData = book.publishData,
                    ImageUrl = book.ImageUrl,
                    Categories = new List<string>(),
                };

                foreach (var c in book.Categories)
                {
                    bookVM.Categories.Add(c.category.Name);
                }
                bookVms.Add(bookVM);
            }

            return View(bookVms); // Return the bookVms list to the view
        }



        [HttpGet]
        public IActionResult Create()
        {
            var authors = context.Authors.OrderBy(author => author.Name).ToList();
            var categories = context.Categories.OrderBy(category => category.Name).ToList();

            var authorList = authors.Select(author => new SelectListItem
            {
                Value = author.Id.ToString(),
                Text = author.Name
            }).ToList();

            var categoryList = categories.Select(category => new SelectListItem
            {
                Value = category.Id.ToString(),
                Text = category.Name
            }).ToList();

            var viewModel = new BookFormVM
            {
                Authors = authorList,
                Categories = categoryList,
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult Create(BookFormVM viewModel)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate authors and categories in case of validation failure
                var authors = context.Authors.OrderBy(author => author.Name).ToList();
                var categories = context.Categories.OrderBy(category => category.Name).ToList();

                viewModel.Authors = authors.Select(author => new SelectListItem
                {
                    Value = author.Id.ToString(),
                    Text = author.Name
                }).ToList();

                viewModel.Categories = categories.Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                }).ToList();

                return View(viewModel);
            }

            var book = new Book
            {
                Title = viewModel.Title,
                AuthorId = viewModel.AuthorId,
                Publisher = viewModel.Publisher,
                publishData = viewModel.publishData,
                Description = viewModel.Description,
                Categories = viewModel.SelectedCategories.Select(id => new BookCategory
                {
                    CategoryId = id,
                }).ToList(),
            };

            context.Books.Add(book);
            context.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
