using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WordStats.Algorithm
{
    public class WordCounter
    {

        public Dictionary<string, int> GetWordStatistics(string text)
        {
            text = text.ToLower();
            string filteredInputText = Regex.Replace(text, "\\W", " ");
            string[] words = Regex.Split(filteredInputText, "\\s+");
            List<string> filteredWords = words.Where(word => !Regex.IsMatch(word, "[0-9]") && word.Length > 3)
                .ToList();
            Dictionary<string, int> statistics = new Dictionary<string, int>();
            foreach (string word in filteredWords)
            {
                if (!statistics.ContainsKey(word))
                {
                    statistics[word] = 0;
                }
                statistics[word]++;
            }
            return statistics;
        }
    }
}
