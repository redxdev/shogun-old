#ifndef _SVM_SHOGUN_VM_H_
#define _SVM_SHOGUN_VM_H_

#include "SVM_Types.h"

#define SVM_LIBRARY

#define SVM_VERSION_STR "0.2.2-dev"
#define SVM_VERSION (Shogun::UInt32)25

#define SVM_HEADER_ID "svmo"

namespace Shogun
{
	const char* version_string();

	UInt32 version();
}

#endif