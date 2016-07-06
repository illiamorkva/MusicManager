using Filters;

namespace MusicManager
{
    public class IspolFilters : IFilter<Sound>
    {
        public string SearchPattern { get; set; }

        public bool Filter(Sound sound)
        {
            return sound.Author.IndexOf(SearchPattern) > -1;
        }
    }

    public class ZanreFilters : IFilter<Sound>
    {
        public string SearchPattern { get; set; }

        public bool Filter(Sound sound)
        {
            return sound.Genre.IndexOf(SearchPattern) > -1;
        }
    }

    public class NameFilters : IFilter<Sound>
    {
        public string SearchPattern { get; set; }

        public bool Filter(Sound sound)
        {
            return sound.Name.IndexOf(SearchPattern) > -1;
        }
    }

    public class YearFilters : IFilter<Sound>
    {
        public string SearchPattern { get; set; }

        public bool Filter(Sound sound)
        {
            return sound.Year.IndexOf(SearchPattern) > -1;
        }
    }

    public class PartialFilters : IFilter<Sound>
    {
        public string SearchPatternIspol { get; set; }
        public string SearchPatternZanre { get; set; }
        public string SearchPatternName { get; set; }
        public string SearchPatternYear { get; set; }

        public bool Filter(Sound sound)
        {
            return sound.Author.IndexOf(SearchPatternIspol) > -1 ||
                  sound.Genre.IndexOf(SearchPatternZanre) > -1 ||
                 sound.Name.IndexOf(SearchPatternName) > -1 ||
                sound.Year.IndexOf(SearchPatternYear) > -1;


        }
    }

    public class FullFilters : IFilter<Sound>
    {
        public string SearchPatternIspol { get; set; }
        public string SearchPatternZanre { get; set; }
        public string SearchPatternName { get; set; }
        public string SearchPatternYear { get; set; }

        public bool Filter(Sound sound)
        {
            return sound.Author.IndexOf(SearchPatternIspol) > -1 &&
                  sound.Genre.IndexOf(SearchPatternZanre) > -1 &&
                 sound.Name.IndexOf(SearchPatternName) > -1 &&
                sound.Year.IndexOf(SearchPatternYear) > -1;


        }
    }
}
