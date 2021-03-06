#include "SVM_Parser.h"
#include "SVM_VirtualMachine.h"

#include <algorithm>

namespace Shogun
{
	namespace Assembler
	{
		Parser::Parser()
		{
		}

		NodeList Parser::parse(TokenStream& tokens)
		{
			NodeList nodes;

			for (TokenStream::iterator it = tokens.begin(); it != tokens.end(); ++it)
			{
				Token token = *it;
				switch (token.type)
				{
				default:
					throw ParseException(token, FORMAT("unexpected token %s", tokenTypeToString(token.type)));

				case TokenType::IDENT:
				{
					++it;
					if (it != tokens.end() && it->type == TokenType::LABEL)
					{
						LabelNode* node = new LabelNode();
						node->setLabel(token.value);
						nodes.push_back(node);
					}
					else
					{
						--it;
						parseOperation(tokens, it, nodes);
					}
					break;
				}

				case TokenType::NEWLINE:
					continue;

				case TokenType::END:
					return nodes;
				}
			}

			return nodes;
		}

		void Parser::parseOperation(TokenStream& tokens, TokenStream::iterator& it, NodeList& nodes)
		{
			Token token = *it;
			String value = token.value;
			std::transform(value.begin(), value.end(), value.begin(), ::toupper);
			OperationNode* node = new OperationNode();

			try
			{
				node->setOpcode(stringToOpcode(value));
			}
			catch (InvalidOperationException)
			{
				throw ParseException(token, "unknown opcode");
			}

			while (it != tokens.end())
			{
				++it;
				token = *it;
				switch (token.type)
				{
				default:
					nodes.push_back(node);
					--it;
					return;

				case NUMBER:
				{
					ObjectPtr object = createObject(token.value);
					object->setNativeType(Object::NUMBER);
					node->getArguments().push_back(object);
					break;
				}

				case ADDRESS:
				{
					ObjectPtr object = createObject(token.value);
					object->setNativeType(Object::ADDRESS);
					node->getArguments().push_back(object);
					break;
				}

				case STRING:
				{
					ObjectPtr object = createObject(token.value);
					node->getArguments().push_back(object);
					break;
				}

				case DEBUG_STR:
				{
					Object::DebugInfo* debug = new Object::DebugInfo();
					debug->string = token.value;
					node->setDebug(debug);
					break;
				}
				}
			}
		}
	}
}