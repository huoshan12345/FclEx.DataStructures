#pragma once

class NonCopyable
{
protected:
	NonCopyable() = default;
	virtual ~NonCopyable() = default;

	NonCopyable(NonCopyable const &) = delete;
	void operator=(NonCopyable const &x) = delete;
};
