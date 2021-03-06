#include "SVM_VirtualMachine.h"
#include "SVM_Opcodes.h"
#include "ShogunVM.h"

#include <fstream>
#include <iomanip>

namespace Shogun
{
	VirtualMachine::VirtualMachine(Memory::MemSize initialMemory)
		: memory(initialMemory)
	{
	}

	VirtualMachine::~VirtualMachine()
	{
	}

	Memory& VirtualMachine::getMemory()
	{
		return this->memory;
	}

	Stack& VirtualMachine::getStack()
	{
		return this->stack;
	}

	VMCallMap& VirtualMachine::getCallMap()
	{
		return this->callMap;
	}

	void VirtualMachine::loadProgram(const Program& program)
	{
		memory.resize(program.size() + 3);
		setRegMmx(program.size() + 2);

		stack.clear();

		UInt32 i = 1;
		for (auto it = program.cbegin(); it != program.cend(); ++it)
		{
			memory.set(i, *it);
			i++;
		}

		setRegPri(1);
	}

	void VirtualMachine::importProgram(const Program& program)
	{
		memory.resize(memory.getSize() + program.size());

		UInt32 i = getRegMmx();
		setRegMmx(memory.getSize());

		for (auto it = program.cbegin(); it != program.cend(); ++it)
		{
			memory.set(i, *it);
			i++;
		}
	}

	void VirtualMachine::run()
	{
		this->running = true;
		while (this->running)
		{
			this->step();
		}
	}

	void VirtualMachine::step()
	{
		UInt32 opcode = memory.get(getRegPri())->getAddress();
		executeOperation(this, (Opcode)opcode);
		this->setRegPri(this->getRegPri() + 1);
	}

	void VirtualMachine::dump()
	{
		std::cerr << "Dumping virtual machine to shogun.dump... ";

		std::ofstream dump;
		dump.open("shogun.dump", std::ios::out | std::ios::trunc);
		if (!dump.is_open()) {
			std::cerr << std::endl << "Unable to open shogun.dump to output dump." << std::endl;
			return;
		}

		dumpStack(dump);
		dumpHeap(dump);

		std::cerr << "done" << std::endl;
	}

	void VirtualMachine::dumpStack(std::ostream& stream)
	{
		auto t = std::time(nullptr);
		auto tm = *std::localtime(&t);

		stream << "ShogunVM version " << Shogun::version_string() << "-" << Shogun::version() << std::endl;
		stream << "dump time - " << std::put_time(&tm, "%Y-%m-%d %H:%M:%S") << std::endl;
		stream << "----------" << std::endl;
		stream << "Registers:" << std::endl;
		stream << "  PRI = " << this->getRegPri() << std::endl;
		stream << "  MMX = " << this->getRegMmx() << std::endl;
		stream << "----------" << std::endl;
		stream << "Stack:" << std::endl;

		for (Stack::iterator it = stack.begin(); it != stack.end(); ++it)
		{
			stream << "> " << (*it)->getReadableString() << std::endl;
		}

		stream << "----------" << std::endl;
		stream << "- end of stack dump" << std::endl;
	}

	void VirtualMachine::dumpHeap(std::ostream& stream)
	{
		auto t = std::time(nullptr);
		auto tm = *std::localtime(&t);

		stream << "ShogunVM version " << Shogun::version_string() << "-" << Shogun::version() << std::endl;
		stream << "dump time - " << std::put_time(&tm, "%Y-%m-%d %H:%M:%S") << std::endl;
		stream << "----------" << std::endl;
		stream << "Registers:" << std::endl;
		stream << "  PRI = " << this->getRegPri() << std::endl;
		stream << "  MMX = " << this->getRegMmx() << std::endl;
		stream << "----------" << std::endl;
		stream << "Heap:" << std::endl;

		for (Memory::MemSize i = 0; i < memory.getSize(); ++i)
		{
			stream << "[" << i << "] " << memory.get(i)->getReadableString();
			if (i == this->getRegPri())
			{
				stream << " !PRI!";
			}
			
			if (i == this->getRegMmx())
			{
				stream << " !MMX!";
			}

			stream << std::endl;
		}

		stream << "- end of heap dump" << std::endl;
	}
}