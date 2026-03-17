using OptiControl.Models.Entities;

namespace OptiControl.Services.IServices;

public interface ITestimonialService
{
    List<Testimonial> GetAll();
    List<Testimonial> GetApprovedForPublic();
    Testimonial? GetById(int id);
    Testimonial Create(Testimonial testimonial);
    bool Update(Testimonial testimonial);
    bool Delete(int id);
    bool SetApproved(int id, bool approved);
}
