using TrybeHotel.Models;
using TrybeHotel.Dto;
using Microsoft.EntityFrameworkCore;

namespace TrybeHotel.Repository
{
    public class RoomRepository : IRoomRepository
    {
        protected readonly ITrybeHotelContext _context;
        public RoomRepository(ITrybeHotelContext context)
        {
            _context = context;
        }

        // 6. Desenvolva o endpoint GET /room/:hotelId
        public IEnumerable<RoomDto> GetRooms(int HotelId)
        {
            throw new NotImplementedException();
        }

        // 7. Desenvolva o endpoint POST /room
        public RoomDto AddRoom(Room room)
        {
            _context.Rooms.Add(room);
            _context.SaveChanges();

            Room roomAdded = _context.Rooms
            .Include(room => room.Hotel)
            .ThenInclude(hotel => hotel.City)
            .First(r => r.Name == room.Name);

            return new RoomDto
            {
                RoomId = roomAdded.RoomId,
                Name = roomAdded.Name,
                Capacity = roomAdded.Capacity,
                Image = roomAdded.Image,
                Hotel = new HotelDto
                {
                    HotelId = roomAdded.HotelId,
                    Name = roomAdded?.Hotel?.Name,
                    Address = roomAdded?.Hotel?.Address,
                    CityId = roomAdded?.Hotel?.CityId,
                    CityName = roomAdded?.Hotel?.City?.Name
                }
            };
        }


        // 8. Desenvolva o endpoint DELETE /room/:roomId
        public void DeleteRoom(int RoomId)
        {
            throw new NotImplementedException();
        }
    }
}