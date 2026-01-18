using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MovieRental.Models.Movies;
using MovieRental.Models.Users;

namespace MovieRental.Data;

/// <summary>
/// Contexto de base de datos principal de la aplicación MovieRental.
/// Hereda de IdentityDbContext para integrar ASP.NET Core Identity con Entity Framework Core.
/// </summary>
/// <remarks>
/// Este contexto gestiona todas las entidades del dominio de la aplicación, incluyendo:
/// - Entidades de Identity (usuarios, roles, claims, etc.)
/// - Dominio Movies: películas, géneros, personas, reparto y equipo técnico
/// - Dominio Users: carritos de compra
/// 
/// Utiliza Fluent API en OnModelCreating para configurar relaciones, índices,
/// claves compuestas y restricciones de base de datos.
/// </remarks>
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ApplicationDbContext"/>.
    /// </summary>
    /// <param name="options">
    /// Las opciones de configuración para el contexto, incluyendo la cadena de conexión
    /// y el proveedor de base de datos (SQL Server, SQLite, etc.).
    /// </param>
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    #region DbSets - Dominio Movies

    /// <summary>
    /// Obtiene el DbSet de películas.
    /// Representa la colección de todas las películas disponibles en el sistema.
    /// </summary>
    /// <value>Una colección queryable de <see cref="Movie"/>.</value>
    public DbSet<Movie> Movies => Set<Movie>();

    /// <summary>
    /// Obtiene el DbSet de géneros cinematográficos.
    /// Representa la colección de géneros disponibles para clasificar películas.
    /// </summary>
    /// <value>Una colección queryable de <see cref="Genre"/>.</value>
    public DbSet<Genre> Genres => Set<Genre>();

    /// <summary>
    /// Obtiene el DbSet de personas (actores, directores, etc.).
    /// Representa la colección de todas las personas involucradas en películas.
    /// </summary>
    /// <value>Una colección queryable de <see cref="Person"/>.</value>
    public DbSet<Person> People => Set<Person>();

    /// <summary>
    /// Obtiene el DbSet de la relación muchos-a-muchos entre películas y géneros.
    /// Representa qué géneros están asociados a cada película.
    /// </summary>
    /// <value>Una colección queryable de <see cref="MovieGenre"/>.</value>
    public DbSet<MovieGenre> MovieGenres => Set<MovieGenre>();

    /// <summary>
    /// Obtiene el DbSet de reparto de películas (actores y sus personajes).
    /// Representa la relación entre películas, actores y los personajes que interpretan.
    /// </summary>
    /// <value>Una colección queryable de <see cref="MovieCast"/>.</value>
    public DbSet<MovieCast> MovieCasts => Set<MovieCast>();

    /// <summary>
    /// Obtiene el DbSet del equipo técnico/creativo de películas.
    /// Representa la relación entre películas y el personal técnico (directores, escritores, etc.).
    /// </summary>
    /// <value>Una colección queryable de <see cref="MovieCrew"/>.</value>
    public DbSet<MovieCrew> MovieCrews => Set<MovieCrew>();

    #endregion

    #region DbSets - Dominio Users

    /// <summary>
    /// Obtiene el DbSet de ítems del carrito de compras.
    /// Representa las películas que los usuarios han agregado a sus carritos.
    /// </summary>
    /// <value>Una colección queryable de <see cref="CartItem"/>.</value>
    public DbSet<CartItem> CartItems => Set<CartItem>();

    #endregion

    /// <summary>
    /// Configura el modelo de datos usando Fluent API.
    /// Define relaciones, claves primarias compuestas, índices y restricciones de base de datos.
    /// </summary>
    /// <param name="modelBuilder">
    /// El constructor de modelos proporcionado por Entity Framework Core
    /// para configurar las entidades y sus relaciones.
    /// </param>
    /// <remarks>
    /// Este método se ejecuta cuando se crea el modelo de datos y configura:
    /// - Claves primarias simples y compuestas
    /// - Propiedades requeridas y longitudes máximas
    /// - Tipos de datos específicos (decimal para precios)
    /// - Relaciones uno-a-muchos y muchos-a-muchos
    /// - Comportamientos de eliminación en cascada
    /// - Índices para optimizar consultas
    /// - Restricciones de unicidad
    /// </remarks>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Llamada base necesaria para configurar las tablas de ASP.NET Core Identity
        base.OnModelCreating(modelBuilder);

        // ========================================
        // Configuración de Movie
        // ========================================
        // Configura la entidad Movie con clave primaria, propiedades requeridas,
        // tipo de columna decimal para precios e índices para optimizar búsquedas
        modelBuilder.Entity<Movie>(entity =>
        {
            // Clave primaria
            entity.HasKey(e => e.MovieId);
            
            // Título: requerido, máximo 200 caracteres
            entity.Property(e => e.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Precio de renta: tipo decimal con 10 dígitos totales, 2 decimales
            entity.Property(e => e.RentalPrice)
                .HasColumnType("decimal(10,2)");

            // Índices para optimizar búsquedas por título y año
            entity.HasIndex(e => e.Title);
            entity.HasIndex(e => e.ReleaseYear);
        });

        // ========================================
        // Configuración de Genre
        // ========================================
        // Configura la entidad Genre con clave primaria, nombre único e índice
        modelBuilder.Entity<Genre>(entity =>
        {
            // Clave primaria
            entity.HasKey(e => e.GenreId);
            
            // Nombre: requerido, máximo 50 caracteres
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(50);

            // Índice único: no puede haber dos géneros con el mismo nombre
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // ========================================
        // Configuración de Person
        // ========================================
        // Configura la entidad Person con clave primaria, nombre completo requerido e índice
        modelBuilder.Entity<Person>(entity =>
        {
            // Clave primaria
            entity.HasKey(e => e.PersonId);
            
            // Nombre completo: requerido, máximo 150 caracteres
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasMaxLength(150);

            // Índice para optimizar búsquedas por nombre
            entity.HasIndex(e => e.FullName);
        });

        // ========================================
        // Configuración de MovieGenre (Many-to-Many)
        // ========================================
        // Configura la relación muchos-a-muchos entre Movie y Genre
        // Una película puede tener múltiples géneros y un género puede estar en múltiples películas
        modelBuilder.Entity<MovieGenre>(entity =>
        {
            // Clave primaria compuesta: combinación única de MovieId y GenreId
            entity.HasKey(mg => new { mg.MovieId, mg.GenreId });

            // Relación con Movie: una entrada MovieGenre pertenece a una Movie,
            // y una Movie tiene muchas entradas MovieGenre
            entity.HasOne(mg => mg.Movie)
                .WithMany(m => m.MovieGenres)
                .HasForeignKey(mg => mg.MovieId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar la película, se eliminan sus géneros

            // Relación con Genre: una entrada MovieGenre pertenece a un Genre,
            // y un Genre tiene muchas entradas MovieGenre
            entity.HasOne(mg => mg.Genre)
                .WithMany(g => g.MovieGenres)
                .HasForeignKey(mg => mg.GenreId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar el género, se eliminan sus asociaciones
        });

        // ========================================
        // Configuración de MovieCast (Many-to-Many con atributos)
        // ========================================
        // Configura la relación muchos-a-muchos entre Movie y Person para el reparto,
        // con información adicional del nombre del personaje interpretado
        modelBuilder.Entity<MovieCast>(entity =>
        {
            // Clave primaria compuesta: combinación única de MovieId y PersonId
            entity.HasKey(mc => new { mc.MovieId, mc.PersonId });

            // Nombre del personaje: requerido, máximo 150 caracteres
            entity.Property(e => e.CharacterName)
                .IsRequired()
                .HasMaxLength(150);

            // Relación con Movie: una entrada MovieCast pertenece a una Movie,
            // y una Movie tiene muchas entradas MovieCast (múltiples actores)
            entity.HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCasts)
                .HasForeignKey(mc => mc.MovieId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar la película, se elimina su reparto

            // Relación con Person: una entrada MovieCast pertenece a una Person (actor),
            // y una Person puede tener muchas entradas MovieCast (múltiples películas)
            entity.HasOne(mc => mc.Person)
                .WithMany(p => p.MovieCasts)
                .HasForeignKey(mc => mc.PersonId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar la persona, se eliminan sus apariciones
        });

        // ========================================
        // Configuración de MovieCrew (Many-to-Many con atributos)
        // ========================================
        // Configura la relación muchos-a-muchos entre Movie y Person para el equipo técnico,
        // con información adicional del rol desempeñado (director, escritor, etc.)
        modelBuilder.Entity<MovieCrew>(entity =>
        {
            // Clave primaria compuesta: combinación única de MovieId, PersonId y Role
            // Incluye Role porque una persona puede tener múltiples roles en la misma película
            entity.HasKey(mc => new { mc.MovieId, mc.PersonId, mc.Role });

            // Rol: requerido, máximo 50 caracteres (Director, Writer, Producer, etc.)
            entity.Property(e => e.Role)
                .IsRequired()
                .HasMaxLength(50);

            // Relación con Movie: una entrada MovieCrew pertenece a una Movie,
            // y una Movie tiene muchas entradas MovieCrew (múltiples miembros del equipo)
            entity.HasOne(mc => mc.Movie)
                .WithMany(m => m.MovieCrews)
                .HasForeignKey(mc => mc.MovieId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar la película, se elimina su equipo

            // Relación con Person: una entrada MovieCrew pertenece a una Person,
            // y una Person puede tener muchas entradas MovieCrew (trabajó en múltiples películas)
            entity.HasOne(mc => mc.Person)
                .WithMany(p => p.MovieCrews)
                .HasForeignKey(mc => mc.PersonId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar la persona, se eliminan sus roles
        });

        // ========================================
        // Configuración de CartItem
        // ========================================
        // Configura la entidad CartItem que representa las películas en el carrito de un usuario
        modelBuilder.Entity<CartItem>(entity =>
        {
            // Clave primaria simple
            entity.HasKey(e => e.CartItemId);

            // Precio al momento de agregar: tipo decimal con 10 dígitos totales, 2 decimales
            entity.Property(e => e.PriceAtAddition)
                .HasColumnType("decimal(10,2)");

            // Relación con ApplicationUser: un CartItem pertenece a un User,
            // y un User puede tener muchos CartItems
            entity.HasOne(c => c.User)
                .WithMany(u => u.CartItems)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar el usuario, se elimina su carrito

            // Relación con Movie: un CartItem referencia a una Movie,
            // pero una Movie no necesita saber qué carritos la contienen (WithMany vacío)
            entity.HasOne(c => c.Movie)
                .WithMany()
                .HasForeignKey(c => c.MovieId)
                .OnDelete(DeleteBehavior.Cascade); // Al eliminar la película, se elimina de los carritos

            // Índice único compuesto: un usuario no puede tener la misma película dos veces en su carrito
            entity.HasIndex(c => new { c.UserId, c.MovieId }).IsUnique();
        });
    }
}