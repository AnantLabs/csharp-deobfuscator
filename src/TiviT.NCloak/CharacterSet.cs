using System;
using System.Text;
using System.Collections.Generic;

namespace TiviT.NCloak
{
    public class CharacterSet
    {
        private readonly char startCharacter;
        private readonly char endCharacter;
		
		private readonly List<char> characterList;
		private int counter=0;

        public CharacterSet(char startCharacter, char endCharacter)
        {
            this.startCharacter = startCharacter;
            this.endCharacter = endCharacter;
			characterList = new List<char>();
        }

      
        public char StartCharacter
        {
            get { return startCharacter; }
        }

      
        public char EndCharacter
        {
            get { return endCharacter; }
        }
        
        public void resetCounter()
        {
        	characterList.Clear();
        	counter=0;
        }

       
        public string Generate()
        {
			counter++;
			return counter.ToString();
        }
    }
}
