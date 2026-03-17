using OptiControl.Data;
using OptiControl.Models.Entities;
using OptiControl.Services.IServices;

namespace OptiControl.Services;

public class TestimonialService : ITestimonialService
{
    private readonly ApplicationDbContext _context;

    public TestimonialService(ApplicationDbContext context) => _context = context;

    public List<Testimonial> GetAll() =>
        _context.Testimonials.OrderBy(t => t.SortOrder).ThenBy(t => t.Id).ToList();

    public List<Testimonial> GetApprovedForPublic() =>
        _context.Testimonials
            .Where(t => t.IsActive && t.IsApproved)
            .OrderBy(t => t.SortOrder).ThenBy(t => t.Id)
            .ToList();

    public Testimonial? GetById(int id) => _context.Testimonials.Find(id);

    public Testimonial Create(Testimonial testimonial)
    {
        testimonial.CreatedAt = DateTime.UtcNow;
        testimonial.UpdatedAt = DateTime.UtcNow;
        NormalizeRating(testimonial);
        _context.Testimonials.Add(testimonial);
        _context.SaveChanges();
        return testimonial;
    }

    public bool Update(Testimonial testimonial)
    {
        var existing = _context.Testimonials.Find(testimonial.Id);
        if (existing == null) return false;
        existing.Quote = testimonial.Quote;
        existing.AuthorName = testimonial.AuthorName;
        existing.Location = testimonial.Location;
        existing.Rating = NormalizeRatingValue(testimonial.Rating);
        existing.SortOrder = testimonial.SortOrder;
        existing.IsActive = testimonial.IsActive;
        existing.IsApproved = testimonial.IsApproved;
        existing.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();
        return true;
    }

    public bool Delete(int id)
    {
        var t = _context.Testimonials.Find(id);
        if (t == null) return false;
        _context.Testimonials.Remove(t);
        _context.SaveChanges();
        return true;
    }

    public bool SetApproved(int id, bool approved)
    {
        var t = _context.Testimonials.Find(id);
        if (t == null) return false;
        t.IsApproved = approved;
        t.UpdatedAt = DateTime.UtcNow;
        _context.SaveChanges();
        return true;
    }

    private static void NormalizeRating(Testimonial t) => t.Rating = NormalizeRatingValue(t.Rating);
    private static int NormalizeRatingValue(int rating) => Math.Clamp(rating, 1, 5);
}
