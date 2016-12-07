#pragma once

#include <functional> 
#include <algorithm>
#include <string>
#include <vector>
#include <sstream>

namespace FclEx
{
	using namespace std;

	class StringHelper
	{
	public:
		template < class T>
		static inline string ToString(const T &value)
		{
			stringstream ss;
			ss << value;
			return ss.str();
		}

		template < class T>
		static inline T FromString(const string &s)
		{
			T value;
			stringstream ss(s);
			ss >> value;
			return value;
		}

		static vector<string> Split(const string& s, const string& delim, const bool keep_empty = false)
		{
			vector<string> result;
			if (delim.empty())
			{
				result.push_back(s);
				return result;
			}
			string::const_iterator substart = s.begin(), subend;
			while (true)
			{
				subend = search(substart, s.end(), delim.begin(), delim.end());
				string temp(substart, subend);
				if (keep_empty || !temp.empty())
				{
					result.push_back(temp);
				}
				if (subend == s.end())
				{
					break;
				}
				substart = subend + delim.size();
			}
			return result;

		}


		static inline string& LeftTrim(string &str)
		{
			string::iterator iter = find_if(str.begin(), str.end(), not1(ptr_fun<int, int>(isspace)));
			str.erase(str.begin(), iter);
			return str;
		}

		static inline string& RightTrim(string &str)
		{
			string::reverse_iterator rev_iter = find_if(str.rbegin(), str.rend(), not1(ptr_fun<int, int>(isspace)));
			str.erase(rev_iter.base(), str.end());
			return str;
		}

		static inline string& Trim(string &st)
		{
			return LeftTrim(RightTrim(st));
		}

		static inline string& ReplaceAll(string &str, const string &old_value, const string &new_value)
		{
			while (true)
			{
				string::size_type   pos(0);
				if ((pos = str.find(old_value)) != string::npos)
					str.replace(pos, old_value.length(), new_value);
				else
					break;
			}
			return str;
		}

		static inline string& ReplaceAllDistinct(string &str, const string&old_value, const string &new_value)
		{
			for (string::size_type pos(0); pos != string::npos; pos += new_value.length())
			{
				if ((pos = str.find(old_value, pos)) != string::npos)
					str.replace(pos, old_value.length(), new_value);
				else
					break;
			}
			return str;
		}

		
	};
}
