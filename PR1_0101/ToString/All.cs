using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows.Controls;
namespace PR1_0101
{
    

    public partial class Category
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Genre
    {
        public override string ToString()
        {
            return Name;
        }
    }

    public partial class Thematics
    {
        public override string ToString()
        {
            return Name;
        }
    }
    public partial class Role
    {
        public override string ToString()
        {
            return Name;
        }
    }
    public partial class GameNights
    {
        public string GamesDisplay
        {
            get
            {
                if (ListOfGames == null) return "";
                return string.Join(", ", ListOfGames
                    .Select(l => l.BoardGames1?.NameGame)
                    .Where(name => !string.IsNullOrEmpty(name))
                );
            }
        }
    }

    public partial class BoardGames
    {
        public override string ToString()
        {
            return NameGame;
        }
    }

    public partial class Users
    {
        public override string ToString()
        {
            return LastName + " " + FirstName;
        }
    }
    public partial class Status
    {
        public override string ToString()
        {
            return Name;
        }
    }
}
