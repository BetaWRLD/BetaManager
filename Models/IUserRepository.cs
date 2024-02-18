using System.Collections.Generic;

namespace BetaManager.Models
{
    public interface IUserRepository
    {
        void Add ( UserModel userModel );
        void Edit ( UserModel userModel );
        void Remove ( int id );
        UserModel GetById ( int id );
        UserModel GetByUsername ( string username );
        IEnumerable<UserModel> GetByAll ();
        //...
    }
}
