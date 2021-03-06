#include "SVM_AsmWriter.h"

#include "ShogunVM.h"

namespace Shogun
{
	namespace Assembler
	{
		AsmWriter::AsmWriter()
		{
		}

		void AsmWriter::write(std::ostream& stream, NodeList& nodes, bool debug)
		{
			writeHeader(stream, nodes, debug);
			writeNodes(stream, nodes, debug);
		}

		void AsmWriter::writeHeader(std::ostream& stream, NodeList& nodes, bool debug)
		{
			stream.write(SVM_HEADER_ID, sizeof(SVM_HEADER_ID));

			UInt32 version = Shogun::version();
			stream.write(reinterpret_cast<char*>(&version), sizeof(version));

			WritableBool dMode = debug;
			stream.write(reinterpret_cast<char*>(&dMode), sizeof(dMode));
		}

		void AsmWriter::writeNodes(std::ostream& stream, NodeList& nodes, bool debug)
		{
			CompileInfo compile;

			for (NodeList::iterator it = nodes.begin(); it != nodes.end(); ++it)
			{
				(*it)->prepass(compile);
			}

			for (NodeList::iterator it = nodes.begin(); it != nodes.end(); ++it)
			{
				(*it)->compile(compile);
			}

			UInt32 compileCount = compile.list.size();
			stream.write(reinterpret_cast<char*>(&compileCount), sizeof(compileCount));

			for (CompileList::iterator it = compile.list.begin(); it != compile.list.end(); ++it)
			{
				(*it)->writeBinary(stream, debug);
			}
		}

		AsmReader::AsmReader()
		{
		}

		CompileInfo AsmReader::read(std::istream& stream)
		{
			readHeader(stream);

			CompileInfo compile;
			readObjects(stream, compile);
			return compile;
		}

		void AsmReader::readHeader(std::istream& stream)
		{
			char header_id[sizeof(SVM_HEADER_ID)];
			stream.read(reinterpret_cast<char*>(&header_id), sizeof(SVM_HEADER_ID));

			if (String(SVM_HEADER_ID) != header_id)
				throw HeaderIdMismatchException("Header id mismatch, expected " SVM_HEADER_ID);

			UInt32 version;
			stream.read(reinterpret_cast<char*>(&version), sizeof(version));

			if (version != Shogun::version())
				throw VersionMismatchException(FORMAT("Version mismatch, expected %u but got %u", Shogun::version(), version));

			WritableBool dMode = false;
			stream.read(reinterpret_cast<char*>(&dMode), sizeof(dMode));

			debug = dMode;
		}

		void AsmReader::readObjects(std::istream& stream, CompileInfo& compile)
		{
			UInt32 objCount;
			stream.read(reinterpret_cast<char*>(&objCount), sizeof(objCount));

			for (UInt32 i = 0; i < objCount; ++i)
			{
				ObjectPtr obj = createObject();
				obj->readBinary(stream, debug);
				compile.list.push_back(obj);
			}
		}
	}
}