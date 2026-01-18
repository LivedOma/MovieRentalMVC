using System.ComponentModel.DataAnnotations;

namespace MovieRental.Models.Movies;

/// <summary>
/// Representa la relación muchos-a-muchos entre películas y personas del equipo técnico/creativo.
/// Esta entidad de unión almacena información sobre el rol específico que desempeña
/// cada persona en una película (director, escritor, productor, etc.).
/// </summary>
/// <remarks>
/// Esta clase actúa como una tabla de unión con payload adicional (Role),
/// implementando el patrón de relación muchos-a-muchos de Entity Framework Core.
/// La clave primaria compuesta está formada por MovieId y PersonId.
/// </remarks>
public class MovieCrew
{
    /// <summary>
    /// Obtiene o establece el identificador de la película.
    /// Forma parte de la clave primaria compuesta junto con PersonId.
    /// </summary>
    /// <value>
    /// El ID único de la película en la que trabaja el miembro del equipo.
    /// </value>
    public int MovieId { get; set; }

    /// <summary>
    /// Obtiene o establece el identificador de la persona.
    /// Forma parte de la clave primaria compuesta junto con MovieId.
    /// </summary>
    /// <value>
    /// El ID único de la persona que forma parte del equipo técnico/creativo.
    /// </value>
    public int PersonId { get; set; }

    /// <summary>
    /// Obtiene o establece el rol que desempeña la persona en la película.
    /// </summary>
    /// <value>
    /// Una cadena que describe el rol específico, como "Director", "Writer", 
    /// "Producer", "Cinematographer", "Editor", "Composer", etc.
    /// </value>
    /// <remarks>
    /// Este campo es obligatorio y tiene una longitud máxima de 50 caracteres.
    /// Permite diferenciar entre múltiples roles que una misma persona puede
    /// tener en una película (por ejemplo, alguien puede ser tanto director como escritor).
    /// </remarks>
    [Required]
    [StringLength(50)]
    public string Role { get; set; } = string.Empty;

    #region Navigation Properties

    /// <summary>
    /// Obtiene o establece la película asociada a este miembro del equipo.
    /// Propiedad de navegación para la relación con la entidad Movie.
    /// </summary>
    /// <value>
    /// La instancia de <see cref="Movie"/> para la cual trabaja este miembro del equipo.
    /// </value>
    /// <remarks>
    /// Esta propiedad no puede ser nula (null!) una vez que la entidad está completamente cargada.
    /// Se utiliza para navegar desde un miembro del equipo hacia su película.
    /// </remarks>
    public Movie Movie { get; set; } = null!;

    /// <summary>
    /// Obtiene o establece la persona asociada a este miembro del equipo.
    /// Propiedad de navegación para la relación con la entidad Person.
    /// </summary>
    /// <value>
    /// La instancia de <see cref="Person"/> que representa al miembro del equipo técnico/creativo.
    /// </value>
    /// <remarks>
    /// Esta propiedad no puede ser nula (null!) una vez que la entidad está completamente cargada.
    /// Se utiliza para navegar desde un rol de equipo hacia la persona que lo desempeña.
    /// </remarks>
    public Person Person { get; set; } = null!;

    #endregion
}