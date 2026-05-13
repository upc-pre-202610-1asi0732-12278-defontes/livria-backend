using LivriaBackend.commerce.Domain.Model.Entities;
using LivriaBackend.commerce.Domain.Repositories;
using LivriaBackend.shared.Infrastructure.Persistence.EFC.Configuration;
using LivriaBackend.shared.Infrastructure.Persistence.EFC.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq; 
using System.Threading.Tasks;

namespace LivriaBackend.commerce.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación concreta del repositorio de <see cref="Review"/> utilizando Entity Framework Core.
    /// Hereda de <see cref="BaseRepository{TEntity}"/> y <see cref="IReviewRepository"/>.
    /// </summary>
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        /// <summary>
        /// Inicializa una nueva instancia de la clase <see cref="ReviewRepository"/>.
        /// </summary>
        /// <param name="context">El contexto de la base de datos de la aplicación.</param>
        public ReviewRepository(AppDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Obtiene una reseña por id con <see cref="Review.UserClient"/> cargado (p. ej. icono en API).
        /// No incluye <see cref="Review.Book"/>: <see cref="Book"/> tiene query filter por <c>IsActive</c> y un Include
        /// con relación requerida puede fallar al materializar si el libro está inactivo.
        /// </summary>
        public new async Task<Review> GetByIdAsync(int id)
        {
            return await this.Context.Reviews
                .IgnoreQueryFilters()
                .Include(r => r.UserClient) 
                .FirstOrDefaultAsync(r => r.Id == id);
        }

        /// <summary>
        /// Obtiene todas las reseñas con <see cref="Review.UserClient"/> (sin Include de Book por query filter).
        /// </summary>
        public new async Task<IEnumerable<Review>> GetAllAsync()
        {
            return await this.Context.Reviews
                .IgnoreQueryFilters()
                .Include(r => r.UserClient) 
                .ToListAsync();
        }

        /// <summary>
        /// Añade una nueva reseña de forma asíncrona al repositorio.
        /// </summary>
        /// <param name="review">El objeto <see cref="Review"/> a añadir.</param>
        /// <returns>Una tarea que representa la operación asíncrona.</returns>
        public async Task AddAsync(Review review)
        {
            await this.Context.Reviews.AddAsync(review);
        }
        
        public async Task UpdateAsync(Review review)
        {
            Context.Set<Review>().Update(review);
            await Task.CompletedTask;
        }

        /// <summary>
        /// Reseñas de un libro con autor (<see cref="Review.UserClient"/>) cargado; sin Include de Book (query filter).
        /// </summary>
        public async Task<IEnumerable<Review>> GetByBookIdAsync(int bookId)
        {
            return await this.Context.Reviews
                .IgnoreQueryFilters()
                .Include(r => r.UserClient) 
                .Where(r => r.BookId == bookId) 
                .ToListAsync();
        }
        
        public async Task<IEnumerable<Review>> GetReviewsByUserIdAsync(int userClientId)
        {
            return await Context.Reviews
                .IgnoreQueryFilters()
                .Include(r => r.UserClient)
                .Where(r => r.UserClientId == userClientId)
                .ToListAsync();
        }
    }
}