using TrybeHotel.Models;
using TrybeHotel.Dto;
using Microsoft.EntityFrameworkCore;

namespace TrybeHotel.Repository
{
    public class HotelRepository : IHotelRepository
    {
        protected readonly ITrybeHotelContext _context;
        public HotelRepository(ITrybeHotelContext context)
        {
            _context = context;
        }

        // 4. Desenvolva o endpoint GET /hotel
        public IEnumerable<HotelDto> GetHotels()
        {
            var hotels = _context.Hotels
                .Include(hotel => hotel.City)
                .ToList()
                .Select(h => new HotelDto
                {
                    HotelId = h.HotelId,
                    Name = h.Name,
                    Address = h.Address,
                    CityId = h.CityId,
                    CityName = h.City?.Name
                });

            return hotels;
        }

        // 5. Desenvolva o endpoint POST /hotel
        public HotelDto AddHotel(Hotel hotel)
        {
            _context.Hotels.Add(hotel);
            _context.SaveChanges();

            var hotelAdded = _context.Hotels
            .Include(h => h.City)
            .First(h => h.Address == hotel.Address);

            return new HotelDto
            {
                HotelId = hotelAdded.HotelId,
                Name = hotelAdded.Name,
                Address = hotelAdded.Address,
                CityId = hotelAdded.CityId,
                CityName = hotelAdded.City?.Name
            };
        }
    }
}