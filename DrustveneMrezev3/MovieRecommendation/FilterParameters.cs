using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DrustveneMrezev3.MovieRecommendation
{
    public class FilterParameters
    {
        private List<string> _filterParameters = new List<string>();
        public FilterMode FilterMode = FilterMode.AND;

        public List<string> GetParameters()
        {
            if (_filterParameters == null)
            {
                return new List<string>();
            }

            return _filterParameters;
        }

        public void AddParameter(string parameter)
        {
            if (_filterParameters == null)
            {
                _filterParameters = new List<string>();
            }

            _filterParameters.Add(parameter);
        }

        public int ParameterCount()
        {
            if (_filterParameters != null)
            {
                return _filterParameters.Count;
            }

            return 0;
        }

        public bool Valid(List<string> parameters)
        {
            if (FilterMode == FilterMode.AND)
            {
                return CheckAnd(parameters);
            }
            else
            {
                return CheckOr(parameters);
            }
        }

        private bool CheckAnd(List<string> parameters)
        {
            foreach (var parameter in _filterParameters)
            {
                bool found = false;
                foreach (var userParameter in parameters)
                {
                    if (parameter.Equals(userParameter))
                    {
                        found = true;
                    }
                }

                if (found == false) //atleast one parameter is false
                {
                    return false;
                }
            }

            return true;
        }

        private bool CheckOr(List<string> parameters)
        {
            foreach (var parameter in _filterParameters)
            {
                bool found = false;
                foreach (var userParameter in parameters)
                {
                    if (parameter.Equals(userParameter))
                    {
                        found = true;
                    }
                }

                if (found == true) //atleast one parameter is true
                {
                    return true;
                }
            }

            return false;
        }

    }

    public enum FilterMode
    {
        AND,OR
    }
}