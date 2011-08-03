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
        
        public void resetCounter()
        {
        	characterList.Clear();
        	counter=0;
        }

       
        public string Generate()
        {
			counter++;
			string zeroPrefix="";
			if (counter<10){
				zeroPrefix="0";	
			}
			string result=zeroPrefix+counter.ToString();
			return result;
        }
    }
}
