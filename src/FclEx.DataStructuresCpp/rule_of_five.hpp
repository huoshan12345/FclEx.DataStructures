#pragma once
#include <string>

namespace Example
{
	/*
	Rule of five
		Because the presence of a user-defined destructor, copy-constructor, or copy-assignment operator prevents implicit definition of the move constructor and the move 
		assignment operator, any class for which move semantics are desirable, has to declare all five special member functions.

		Unlike Rule of Three, failing to provide move constructor and move assignment is usually not an error, but a missed optimization opportunity.
	*/


	class rule_of_five
	{
		char* cstring; // raw pointer used as a handle to a dynamically-allocated memory block
	public:
		rule_of_five(const char* arg)
			: cstring(new char[std::strlen(arg) + 1]) // allocate
		{
			std::strcpy(cstring, arg); // populate
		}

		~rule_of_five()
		{
			delete[] cstring;  // deallocate
		}

		rule_of_five(const rule_of_five& other) // copy constructor
		{
			cstring = new char[std::strlen(other.cstring) + 1];
			std::strcpy(cstring, other.cstring);
		}

		rule_of_five(rule_of_five&& other) noexcept : cstring(other.cstring) // move constructor
		{
			other.cstring = nullptr;
		}

		rule_of_five& operator=(const rule_of_five& other) // copy assignment
		{
			char* tmp_cstring = new char[std::strlen(other.cstring) + 1];
			std::strcpy(tmp_cstring, other.cstring);
			delete[] cstring;
			cstring = tmp_cstring;
			return *this;
		}

		rule_of_five& operator=(rule_of_five&& other) noexcept
		// move assignment
		{
			delete[] cstring;
			cstring = other.cstring;
			other.cstring = nullptr;
			return *this;
		}
		// alternatively, replace both assignment operators with 
		//  rule_of_five& operator=(rule_of_five other)
		//  {
		//      std::swap(cstring, other.cstring);
		//      return *this;
		//  }
	};
}