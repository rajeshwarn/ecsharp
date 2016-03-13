// Generated from EcsParserGrammar.les by LeMP custom tool. LeMP version: 1.6.1.0
// Note: you can give command-line arguments to the tool via 'Custom Tool Namespace':
// --no-out-header       Suppress this message
// --verbose             Allow verbose messages (shown by VS as 'warnings')
// --timeout=X           Abort processing thread after X seconds (default: 10)
// --macros=FileName.dll Load macros from FileName.dll, path relative to this file 
// Use #importMacros to use macros in a given namespace, e.g. #importMacros(Loyc.LLPG);
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Loyc;
using Loyc.Syntax;
using Loyc.Syntax.Les;
using Loyc.Syntax.Lexing;
using Loyc.Collections;
using Loyc.Collections.Impl;
namespace Loyc.Ecs.Parser
{
	using TT = TokenType;
	using S = CodeSymbols;
	using EP = EcsPrecedence;
	
	#pragma warning disable 162, 642
	
	partial class EcsParser
	{
		enum StmtCat
		{
			MethodOrPropOrVar = 0, KeywordStmt = 1, IdStmt = 2, ThisConstructor = 3, OtherStmt = 4
		}
		StmtCat DetectStatementCategory(out int wordAttrCount)
		{
			wordAttrCount = 0;
			int wordsStartAt = InputPosition;
			bool haveNew = LA0 == TT.@new;
			if ((haveNew || LA0 == TT.Id || LA0 == TT.ContextualKeyword)) {
				bool isAttrKw = haveNew;
				do {
					Skip();
					if ((isAttrKw)) {
						wordsStartAt = InputPosition;
					} else {
						wordAttrCount++;
					}
					haveNew |= (isAttrKw = (LA0 == TT.@new));
				} while (((isAttrKw |= LA0 == TT.AttrKeyword || LA0 == TT.@new) || LA0 == TT.Id || LA0 == TT.ContextualKeyword));
			}
			int consecutive = InputPosition - wordsStartAt;
			if ((LA0 != TT.Substitute)) {
				if ((LA0 == TT.TypeKeyword)) {
					var LA1 = LA(1);
					if ((LA1 == TT.Id || LA1 == TT.ContextualKeyword)) {
						return StmtCat.MethodOrPropOrVar;
					} else {
						consecutive++;
					}
				} else if ((LA0 == TT.@this)) {
					if (LA(1) == TT.LParen && LA(2) == TT.RParen) {
						TT la3 = LA(3);
						if (la3 == TT.Colon || la3 == TT.LBrace || la3 == TT.Semicolon && _spaceName != S.Fn) {
							return StmtCat.ThisConstructor;
						} else {
							return StmtCat.OtherStmt;
						}
					} else if ((consecutive != 0)) {
						InputPosition--;
						return StmtCat.MethodOrPropOrVar;
					}
				} else if ((LT0.Kind == TokenKind.OtherKeyword)) {
					if ((LA0 != TT.@if)) {
						if ((LA0 == TT.@namespace || LA0 == TT.@class || LA0 == TT.@struct || LA0 == TT.@interface || LA0 == TT.@enum || LA0 == TT.@using || LA0 == TT.@event || LA0 == TT.@case || LA0 == TT.@switch || LA0 == TT.@while || LA0 == TT.@fixed || LA0 == TT.@for || LA0 == TT.@foreach || LA0 == TT.@goto || LA0 == TT.@lock || LA0 == TT.@delegate && LA(1) != TT.LParen || LA0 == TT.@do || LA0 == TT.@return || LA0 == TT.@break || LA0 == TT.@try || LA0 == TT.@continue || LA0 == TT.@throw || (LA0 == TT.@checked || LA0 == TT.@unchecked) && LA(1) == TT.LBrace)) {
							return StmtCat.KeywordStmt;
						}
					}
				} else if ((consecutive == 0)) {
					return StmtCat.OtherStmt;
				}
				if ((consecutive >= 2)) {
					InputPosition = wordsStartAt;
					return StmtCat.MethodOrPropOrVar;
				} else if (((LA0 == TT.LParen || LA0 == TT.Set) && !haveNew)) {
					InputPosition = wordsStartAt;
					return consecutive != 0 ? StmtCat.IdStmt : StmtCat.OtherStmt;
				}
			}
			InputPosition = wordsStartAt;
			using (new SavePosition(this, 0)) {
				if ((Scan_DataType(false) && Scan_ComplexNameDecl() && (LA0 == TT.Set || LA0 == TT.Semicolon || LA0 == TT.LBrace || LA0 == TT.LParen || LA0 == TT.LBrack || LA0 == TT.Comma))) {
					return StmtCat.MethodOrPropOrVar;
				}
			}
			if ((haveNew)) {
				if ((LA(-1) == TT.@new)) {
					InputPosition--;
				} else {
					wordAttrCount++;
				}
			}
			return consecutive != 0 ? StmtCat.IdStmt : StmtCat.OtherStmt;
		}
		static readonly Symbol _trait = GSymbol.Get("trait");
		static readonly Symbol _alias = GSymbol.Get("alias");
		static readonly Symbol _where = GSymbol.Get("where");
		static readonly Symbol _when = GSymbol.Get("when");
		static readonly Symbol _assembly = GSymbol.Get("assembly");
		static readonly Symbol _module = GSymbol.Get("module");
		static readonly Symbol _from = GSymbol.Get("from");
		static readonly Symbol _await = GSymbol.Get("await");
		Symbol _spaceName;
		bool _insideLinqExpr;
		internal static readonly HashSet<object> LinqKeywords = EcsLexer.LinqKeywords;
		LNode DataType(bool afterAsOrIs = false)
		{
			Token? brack;
			var type = DataType(afterAsOrIs, out brack);
			if ((brack != null)) {
				Error("A type name cannot include [array dimensions]. The square brackets should be empty.");
			}
			return type;
		}
		static LNode AutoRemoveParens(LNode node)
		{
			int i = node.Attrs.IndexWithName(S.TriviaInParens);
			if ((i > -1)) {
				return node.WithAttrs(node.Attrs.RemoveAt(i));
			}
			return node;
		}
		int count;
		LNode ComplexNameDecl()
		{
			bool _;
			return ComplexNameDecl(false, out _);
		}
		public static readonly Precedence ContinueExpr = new Precedence(-100);
		LNode TypeInside(Token args)
		{
			if ((!Down(args)))
				 return F.Id(S.Missing, args.EndIndex, args.EndIndex);
			var type = DataType();
			Match((int) EOF);
			return Up(type);
		}
		LNode SetOperatorStyle(LNode node)
		{
			return node.SetBaseStyle(NodeStyle.Operator);
		}
		LNode SetAlternateStyle(LNode node)
		{
			node.Style |= NodeStyle.Alternate;
			return node;
		}
		void NonKeywordAttrError(IList<LNode> attrs, string stmtType)
		{
			var attr = attrs.FirstOrDefault(a => a.AttrNamed(S.TriviaWordAttribute) != null);
			if ((attr != null)) {
				Error(attr, "'{0}' appears to be a word attribute, which is not permitted before '{1}'", attr.Range.SourceText, stmtType);
			}
		}
		static readonly Symbol _var = GSymbol.Get("var");
		static readonly Symbol _dynamic = GSymbol.Get("dynamic");
		private void MaybeRecognizeVarAsKeyword(ref LNode type)
		{
			SourceRange rng;
			Symbol name = type.Name;
			if ((name == _var || name == _dynamic) && type.IsId && (rng = type.Range).Source.Text.TryGet(rng.StartIndex, '\0') != '@') {
				type = type.WithName(name == _var ? S.Missing : S.Dynamic);
			}
		}
		bool IsNamedArg(LNode node)
		{
			return node.Calls(S.NamedArg, 2) && node.BaseStyle == NodeStyle.Operator;
		}
		WList<LNode> _stmtAttrs = new WList<LNode>();
		LNode CoreName(LNode complexId)
		{
			if (complexId.IsId)
				 return complexId;
			if (complexId.CallsMin(S.Of, 1))
				 return CoreName(complexId.Args[0]);
			if (complexId.CallsMin(S.Dot, 1))
				 return complexId.Args.Last;
			if (complexId.CallsMin(S.Substitute, 1))
				 return complexId;
			Debug.Fail("Not a complex identifier");
			return complexId.Target;
		}
		Symbol TParamSymbol(LNode T)
		{
			if (T.IsId)
				 return T.Name;
			else if (T.Calls(S.Substitute, 1) && T.Args[0].IsId)
				 return T.Args[0].Name;
			else
				 return S.Missing;
		}
		bool Is(int li, Symbol value)
		{
			return LT(li).Value == value;
		}
		private LNode MethodBodyOrForward()
		{
			LNode _;
			return MethodBodyOrForward(false, out _);
		}
		bool IsArrayType(LNode type)
		{
			return type.Calls(S.Of, 2) && S.IsArrayKeyword(type.Args[0].Name);
		}
		LNode ArgList(Token lp, Token rp)
		{
			var list = new WList<LNode>();
			if ((Down(lp.Children))) {
				ArgList(list);
				Up();
			}
			return F.List(list.ToVList(), lp.StartIndex, rp.EndIndex);
		}
		int ColumnOf(int index)
		{
			return _sourceFile.IndexToLine(index).PosInLine;
		}
		LNode MissingHere()
		{
			var i = GetTextPosition(InputPosition);
			return F.Id(S.Missing, i, i);
		}
		Token UnusualId()
		{
			Check(!(_insideLinqExpr && LinqKeywords.Contains(LT(0).Value)), "!(_insideLinqExpr && LinqKeywords.Contains(LT($LI).Value))");
			var t = Match((int) TT.ContextualKeyword);
			// line 203
			return t;
		}
		bool Scan_UnusualId()
		{
			if (_insideLinqExpr && LinqKeywords.Contains(LT(0).Value))
				return false;
			if (!TryMatch((int) TT.ContextualKeyword))
				return false;
			return true;
		}
		LNode DataType(bool afterAsOrIs, out Token? majorDimension)
		{
			LNode result = default(LNode);
			result = ComplexId();
			TypeSuffixOpt(afterAsOrIs, out majorDimension, ref result);
			return result;
		}
		bool Try_Scan_DataType(int lookaheadAmt, bool afterAsOrIs = false)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_DataType(afterAsOrIs);
		}
		bool Scan_DataType(bool afterAsOrIs = false)
		{
			if (!Scan_ComplexId())
				return false;
			if (!Scan_TypeSuffixOpt(afterAsOrIs))
				return false;
			return true;
		}
		LNode ComplexId()
		{
			TokenType la0, la1;
			var e = IdAtom();
			// Line 248: (TT.ColonColon IdAtom)?
			do {
				la0 = LA0;
				if (la0 == TT.ColonColon) {
					switch (LA(1)) {
					case TT.@operator:
					case TT.Id:
					case TT.Substitute:
					case TT.TypeKeyword:
						goto match1;
					case TT.ContextualKeyword:
						{
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
								goto match1;
						}
						break;
					}
				}
				break;
			match1:
				{
					Skip();
					var e2 = IdAtom();
					// line 249
					e = F.Call(S.ColonColon, e, e2, e.Range.StartIndex, e2.Range.EndIndex);
				}
			} while (false);
			// Line 251: (TParams)?
			la0 = LA0;
			if (la0 == TT.LT) {
				switch (LA(1)) {
				case TT.@operator:
				case TT.Id:
				case TT.Substitute:
				case TT.TypeKeyword:
					TParams(ref e);
					break;
				case TT.ContextualKeyword:
					{
						if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
							TParams(ref e);
					}
					break;
				case TT.GT:
					TParams(ref e);
					break;
				}
			} else if (la0 == TT.Dot) {
				la1 = LA(1);
				if (la1 == TT.LBrack)
					TParams(ref e);
			} else if (la0 == TT.Not) {
				la1 = LA(1);
				if (la1 == TT.LParen)
					TParams(ref e);
			}
			// Line 252: (TT.Dot IdAtom (TParams)?)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.Dot) {
					switch (LA(1)) {
					case TT.@operator:
					case TT.Id:
					case TT.Substitute:
					case TT.TypeKeyword:
						goto match1_a;
					case TT.ContextualKeyword:
						{
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
								goto match1_a;
							else
								goto stop;
						}
					default:
						goto stop;
					}
				} else
					goto stop;
			match1_a:
				{
					Skip();
					var rhs = IdAtom();
					// line 252
					e = F.Dot(e, rhs);
					// Line 253: (TParams)?
					la0 = LA0;
					if (la0 == TT.LT) {
						switch (LA(1)) {
						case TT.@operator:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							TParams(ref e);
							break;
						case TT.ContextualKeyword:
							{
								if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
									TParams(ref e);
							}
							break;
						case TT.GT:
							TParams(ref e);
							break;
						}
					} else if (la0 == TT.Dot) {
						la1 = LA(1);
						if (la1 == TT.LBrack)
							TParams(ref e);
					} else if (la0 == TT.Not) {
						la1 = LA(1);
						if (la1 == TT.LParen)
							TParams(ref e);
					}
				}
			}
		stop:;
			// line 255
			return e;
		}
		bool Scan_ComplexId()
		{
			TokenType la0, la1;
			if (!Scan_IdAtom())
				return false;
			// Line 248: (TT.ColonColon IdAtom)?
			do {
				la0 = LA0;
				if (la0 == TT.ColonColon) {
					switch (LA(1)) {
					case TT.@operator:
					case TT.Id:
					case TT.Substitute:
					case TT.TypeKeyword:
						goto match1;
					case TT.ContextualKeyword:
						{
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
								goto match1;
						}
						break;
					}
				}
				break;
			match1:
				{
					if (!TryMatch((int) TT.ColonColon))
						return false;
					if (!Scan_IdAtom())
						return false;
				}
			} while (false);
			// Line 251: (TParams)?
			do {
				la0 = LA0;
				if (la0 == TT.LT) {
					switch (LA(1)) {
					case TT.@operator:
					case TT.Id:
					case TT.Substitute:
					case TT.TypeKeyword:
						goto matchTParams;
					case TT.ContextualKeyword:
						{
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
								goto matchTParams;
						}
						break;
					case TT.GT:
						goto matchTParams;
					}
				} else if (la0 == TT.Dot) {
					la1 = LA(1);
					if (la1 == TT.LBrack)
						goto matchTParams;
				} else if (la0 == TT.Not) {
					la1 = LA(1);
					if (la1 == TT.LParen)
						goto matchTParams;
				}
				break;
			matchTParams:
				{
					if (!Scan_TParams())
						return false;
				}
			} while (false);
			// Line 252: (TT.Dot IdAtom (TParams)?)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.Dot) {
					switch (LA(1)) {
					case TT.@operator:
					case TT.Id:
					case TT.Substitute:
					case TT.TypeKeyword:
						goto match1_a;
					case TT.ContextualKeyword:
						{
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
								goto match1_a;
							else
								goto stop;
						}
					default:
						goto stop;
					}
				} else
					goto stop;
			match1_a:
				{
					if (!TryMatch((int) TT.Dot))
						return false;
					if (!Scan_IdAtom())
						return false;
					// Line 253: (TParams)?
					do {
						la0 = LA0;
						if (la0 == TT.LT) {
							switch (LA(1)) {
							case TT.@operator:
							case TT.Id:
							case TT.Substitute:
							case TT.TypeKeyword:
								goto matchTParams_a;
							case TT.ContextualKeyword:
								{
									if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
										goto matchTParams_a;
								}
								break;
							case TT.GT:
								goto matchTParams_a;
							}
						} else if (la0 == TT.Dot) {
							la1 = LA(1);
							if (la1 == TT.LBrack)
								goto matchTParams_a;
						} else if (la0 == TT.Not) {
							la1 = LA(1);
							if (la1 == TT.LParen)
								goto matchTParams_a;
						}
						break;
					matchTParams_a:
						{
							if (!Scan_TParams())
								return false;
						}
					} while (false);
				}
			}
		stop:;
			return true;
		}
		LNode IdAtom()
		{
			// line 260
			LNode r;
			// Line 261: ( TT.Substitute Atom | TT.@operator AnyOperator | (TT.Id|TT.TypeKeyword) | UnusualId )
			switch (LA0) {
			case TT.Substitute:
				{
					var t = MatchAny();
					var e = Atom();
					e = AutoRemoveParens(e);
					r = F.Call(S.Substitute, e, t.StartIndex, e.Range.EndIndex);
				}
				break;
			case TT.@operator:
				{
					var op = MatchAny();
					var t = AnyOperator();
					// line 264
					r = F.Attr(_triviaUseOperatorKeyword, F.Id((Symbol) t.Value, op.StartIndex, t.EndIndex));
				}
				break;
			case TT.Id:
			case TT.TypeKeyword:
				{
					var t = MatchAny();
					// line 266
					r = F.Id(t);
				}
				break;
			default:
				{
					var t = UnusualId();
					// line 268
					r = F.Id(t);
				}
				break;
			}
			// line 269
			return r;
		}
		bool Scan_IdAtom()
		{
			// Line 261: ( TT.Substitute Atom | TT.@operator AnyOperator | (TT.Id|TT.TypeKeyword) | UnusualId )
			switch (LA0) {
			case TT.Substitute:
				{
					if (!TryMatch((int) TT.Substitute))
						return false;
					if (!Scan_Atom())
						return false;
				}
				break;
			case TT.@operator:
				{
					if (!TryMatch((int) TT.@operator))
						return false;
					if (!Scan_AnyOperator())
						return false;
				}
				break;
			case TT.Id:
			case TT.TypeKeyword:
				if (!TryMatch((int) TT.Id, (int) TT.TypeKeyword))
					return false;
				break;
			default:
				if (!Scan_UnusualId())
					return false;
				break;
			}
			return true;
		}
		void TParams(ref LNode r)
		{
			TokenType la0;
			WList<LNode> list = new WList<LNode> { 
				r
			};
			Token end;
			// Line 286: ( TT.LT (DataType (TT.Comma DataType)*)? TT.GT | TT.Dot TT.LBrack TT.RBrack | TT.Not TT.LParen TT.RParen )
			la0 = LA0;
			if (la0 == TT.LT) {
				Skip();
				// Line 286: (DataType (TT.Comma DataType)*)?
				switch (LA0) {
				case TT.@operator:
				case TT.ContextualKeyword:
				case TT.Id:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						list.Add(DataType());
						// Line 286: (TT.Comma DataType)*
						for (;;) {
							la0 = LA0;
							if (la0 == TT.Comma) {
								Skip();
								list.Add(DataType());
							} else
								break;
						}
					}
					break;
				}
				end = Match((int) TT.GT);
			} else if (la0 == TT.Dot) {
				Skip();
				var t = Match((int) TT.LBrack);
				end = Match((int) TT.RBrack);
				// line 287
				list = AppendExprsInside(t, list);
			} else {
				Match((int) TT.Not);
				var t = Match((int) TT.LParen);
				end = Match((int) TT.RParen);
				// line 288
				list = AppendExprsInside(t, list);
			}
			// line 291
			int start = r.Range.StartIndex;
			r = F.Call(S.Of, list.ToVList(), start, end.EndIndex);
		}
		bool Try_Scan_TParams(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_TParams();
		}
		bool Scan_TParams()
		{
			TokenType la0;
			// Line 286: ( TT.LT (DataType (TT.Comma DataType)*)? TT.GT | TT.Dot TT.LBrack TT.RBrack | TT.Not TT.LParen TT.RParen )
			la0 = LA0;
			if (la0 == TT.LT) {
				if (!TryMatch((int) TT.LT))
					return false;
				// Line 286: (DataType (TT.Comma DataType)*)?
				switch (LA0) {
				case TT.@operator:
				case TT.ContextualKeyword:
				case TT.Id:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						if (!Scan_DataType())
							return false;
						// Line 286: (TT.Comma DataType)*
						for (;;) {
							la0 = LA0;
							if (la0 == TT.Comma) {
								if (!TryMatch((int) TT.Comma))
									return false;
								if (!Scan_DataType())
									return false;
							} else
								break;
						}
					}
					break;
				}
				if (!TryMatch((int) TT.GT))
					return false;
			} else if (la0 == TT.Dot) {
				if (!TryMatch((int) TT.Dot))
					return false;
				if (!TryMatch((int) TT.LBrack))
					return false;
				if (!TryMatch((int) TT.RBrack))
					return false;
			} else {
				if (!TryMatch((int) TT.Not))
					return false;
				if (!TryMatch((int) TT.LParen))
					return false;
				if (!TryMatch((int) TT.RParen))
					return false;
			}
			return true;
		}
		bool TypeSuffixOpt(bool afterAsOrIs, out Token? dimensionBrack, ref LNode e)
		{
			TokenType la0, la1;
			// line 301
			int count;
			bool result = false;
			dimensionBrack = null;
			// Line 334: greedy( TT.QuestionMark (&!{afterAsOrIs} | &!(((TT.@new|TT.Add|TT.AndBits|TT.At|TT.Forward|TT.Id|TT.IncDec|TT.LBrace|TT.Literal|TT.LParen|TT.Mul|TT.Not|TT.NotBits|TT.Sub|TT.Substitute|TT.TypeKeyword) | UnusualId))) | TT.Mul | &{(count = CountDims(LT($LI), @true)) > 0} TT.LBrack TT.RBrack greedy(&{(count = CountDims(LT($LI), @false)) > 0} TT.LBrack TT.RBrack)* )*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.QuestionMark) {
					if (!afterAsOrIs)
						goto match1;
					else if ((count = CountDims(LT(1), true)) > 0) {
						if (!Try_TypeSuffixOpt_Test0(1))
							goto match1;
						else
							break;
					} else if (!Try_TypeSuffixOpt_Test0(1))
						goto match1;
					else
						break;
				} else if (la0 == TT.Mul) {
					var t = MatchAny();
					// line 340
					e = F.Of(F.Id(t), e, e.Range.StartIndex, t.EndIndex);
					result = true;
				} else if (la0 == TT.LBrack) {
					if ((count = CountDims(LT(0), true)) > 0) {
						la1 = LA(1);
						if (la1 == TT.RBrack) {
							var dims = InternalList<Pair<int,int>>.Empty;
							Token rb;
							var lb = MatchAny();
							rb = MatchAny();
							// line 345
							dims.Add(Pair.Create(count, rb.EndIndex));
							// Line 346: greedy(&{(count = CountDims(LT($LI), @false)) > 0} TT.LBrack TT.RBrack)*
							for (;;) {
								la0 = LA0;
								if (la0 == TT.LBrack) {
									if ((count = CountDims(LT(0), false)) > 0) {
										la1 = LA(1);
										if (la1 == TT.RBrack) {
											Skip();
											rb = MatchAny();
											// line 346
											dims.Add(Pair.Create(count, rb.EndIndex));
										} else
											break;
									} else
										break;
								} else
									break;
							}
							// line 348
							if (CountDims(lb, false) <= 0) {
								dimensionBrack = lb;
							}
							for (int i = dims.Count - 1; i >= 0; i--) {
								e = F.Of(F.Id(S.GetArrayKeyword(dims[i].A)), e, e.Range.StartIndex, dims[i].B);
							}
							result = true;
						} else
							break;
					} else
						break;
				} else
					break;
				continue;
			match1:
				{
					var t = MatchAny();
					// Line 334: (&!{afterAsOrIs} | &!(((TT.@new|TT.Add|TT.AndBits|TT.At|TT.Forward|TT.Id|TT.IncDec|TT.LBrace|TT.Literal|TT.LParen|TT.Mul|TT.Not|TT.NotBits|TT.Sub|TT.Substitute|TT.TypeKeyword) | UnusualId)))
					if (!afterAsOrIs) {
					} else
						Check(!Try_TypeSuffixOpt_Test0(0), "!(((TT.@new|TT.Add|TT.AndBits|TT.At|TT.Forward|TT.Id|TT.IncDec|TT.LBrace|TT.Literal|TT.LParen|TT.Mul|TT.Not|TT.NotBits|TT.Sub|TT.Substitute|TT.TypeKeyword) | UnusualId))");
					// line 337
					e = F.Of(F.Id(t), e, e.Range.StartIndex, t.EndIndex);
					result = true;
				}
			}
			// line 357
			return result;
		}
		bool Try_Scan_TypeSuffixOpt(int lookaheadAmt, bool afterAsOrIs)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_TypeSuffixOpt(afterAsOrIs);
		}
		bool Scan_TypeSuffixOpt(bool afterAsOrIs)
		{
			TokenType la0, la1;
			// Line 334: greedy( TT.QuestionMark (&!{afterAsOrIs} | &!(((TT.@new|TT.Add|TT.AndBits|TT.At|TT.Forward|TT.Id|TT.IncDec|TT.LBrace|TT.Literal|TT.LParen|TT.Mul|TT.Not|TT.NotBits|TT.Sub|TT.Substitute|TT.TypeKeyword) | UnusualId))) | TT.Mul | &{(count = CountDims(LT($LI), @true)) > 0} TT.LBrack TT.RBrack greedy(&{(count = CountDims(LT($LI), @false)) > 0} TT.LBrack TT.RBrack)* )*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.QuestionMark) {
					if (!afterAsOrIs)
						goto match1;
					else if ((count = CountDims(LT(1), true)) > 0) {
						if (!Try_TypeSuffixOpt_Test0(1))
							goto match1;
						else
							break;
					} else if (!Try_TypeSuffixOpt_Test0(1))
						goto match1;
					else
						break;
				} else if (la0 == TT.Mul)
					{if (!TryMatch((int) TT.Mul))
						return false;}
				else if (la0 == TT.LBrack) {
					if ((count = CountDims(LT(0), true)) > 0) {
						la1 = LA(1);
						if (la1 == TT.RBrack) {
							if (!TryMatch((int) TT.LBrack))
								return false;
							if (!TryMatch((int) TT.RBrack))
								return false;
							// Line 346: greedy(&{(count = CountDims(LT($LI), @false)) > 0} TT.LBrack TT.RBrack)*
							for (;;) {
								la0 = LA0;
								if (la0 == TT.LBrack) {
									if ((count = CountDims(LT(0), false)) > 0) {
										la1 = LA(1);
										if (la1 == TT.RBrack) {
											if (!TryMatch((int) TT.LBrack))
												return false;
											if (!TryMatch((int) TT.RBrack))
												return false;
										} else
											break;
									} else
										break;
								} else
									break;
							}
						} else
							break;
					} else
						break;
				} else
					break;
				continue;
			match1:
				{
					if (!TryMatch((int) TT.QuestionMark))
						return false;
					// Line 334: (&!{afterAsOrIs} | &!(((TT.@new|TT.Add|TT.AndBits|TT.At|TT.Forward|TT.Id|TT.IncDec|TT.LBrace|TT.Literal|TT.LParen|TT.Mul|TT.Not|TT.NotBits|TT.Sub|TT.Substitute|TT.TypeKeyword) | UnusualId)))
					if (!afterAsOrIs) {
					} else if (Try_TypeSuffixOpt_Test0(0))
						return false;
				}
			}
			return true;
		}
		LNode ComplexNameDecl(bool thisAllowed, out bool hasThis)
		{
			TokenType la0, la1;
			LNode e = default(LNode);
			LNode got_ComplexThisDecl = default(LNode);
			// Line 367: (ComplexThisDecl | IdAtom (TT.ColonColon IdAtom)? (TParamsDecl)? (TT.Dot IdAtom (TParamsDecl)?)* (TT.Dot ComplexThisDecl)?)
			la0 = LA0;
			if (la0 == TT.@this) {
				e = ComplexThisDecl(thisAllowed);
				// line 367
				hasThis = true;
			} else {
				e = IdAtom();
				// line 368
				hasThis = false;
				// Line 370: (TT.ColonColon IdAtom)?
				do {
					la0 = LA0;
					if (la0 == TT.ColonColon) {
						switch (LA(1)) {
						case TT.@operator:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							goto match1;
						case TT.ContextualKeyword:
							{
								if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
									goto match1;
							}
							break;
						}
					}
					break;
				match1:
					{
						Skip();
						var e2 = IdAtom();
						// line 371
						e = F.Call(S.ColonColon, e, e2, e.Range.StartIndex, e2.Range.EndIndex);
					}
				} while (false);
				// Line 373: (TParamsDecl)?
				la0 = LA0;
				if (la0 == TT.LT) {
					switch (LA(1)) {
					case TT.@operator:
					case TT.Id:
					case TT.Substitute:
					case TT.TypeKeyword:
						TParamsDecl(ref e);
						break;
					case TT.ContextualKeyword:
						{
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
								TParamsDecl(ref e);
						}
						break;
					case TT.LBrack:
						{
							if (!(Down(1) && Up(Try_Scan_AsmOrModLabel(0))))
								TParamsDecl(ref e);
						}
						break;
					case TT.@in:
					case TT.AttrKeyword:
					case TT.GT:
						TParamsDecl(ref e);
						break;
					}
				} else if (la0 == TT.Dot) {
					la1 = LA(1);
					if (la1 == TT.LBrack)
						TParamsDecl(ref e);
				} else if (la0 == TT.Not) {
					la1 = LA(1);
					if (la1 == TT.LParen)
						TParamsDecl(ref e);
				}
				// Line 374: (TT.Dot IdAtom (TParamsDecl)?)*
				for (;;) {
					la0 = LA0;
					if (la0 == TT.Dot) {
						switch (LA(1)) {
						case TT.@operator:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							goto match1_a;
						case TT.ContextualKeyword:
							{
								if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
									goto match1_a;
								else
									goto stop;
							}
						default:
							goto stop;
						}
					} else
						goto stop;
				match1_a:
					{
						Skip();
						var rhs = IdAtom();
						// line 375
						e = F.Dot(e, rhs);
						// Line 376: (TParamsDecl)?
						la0 = LA0;
						if (la0 == TT.LT) {
							switch (LA(1)) {
							case TT.@operator:
							case TT.Id:
							case TT.Substitute:
							case TT.TypeKeyword:
								TParamsDecl(ref e);
								break;
							case TT.ContextualKeyword:
								{
									if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
										TParamsDecl(ref e);
								}
								break;
							case TT.LBrack:
								{
									if (!(Down(1) && Up(Try_Scan_AsmOrModLabel(0))))
										TParamsDecl(ref e);
								}
								break;
							case TT.@in:
							case TT.AttrKeyword:
							case TT.GT:
								TParamsDecl(ref e);
								break;
							}
						} else if (la0 == TT.Dot) {
							la1 = LA(1);
							if (la1 == TT.LBrack)
								TParamsDecl(ref e);
						} else if (la0 == TT.Not) {
							la1 = LA(1);
							if (la1 == TT.LParen)
								TParamsDecl(ref e);
						}
					}
				}
			stop:;
				// Line 378: (TT.Dot ComplexThisDecl)?
				la0 = LA0;
				if (la0 == TT.Dot) {
					la1 = LA(1);
					if (la1 == TT.@this) {
						Skip();
						got_ComplexThisDecl = ComplexThisDecl(thisAllowed);
						hasThis = true;
						e = F.Dot(e, got_ComplexThisDecl);
					}
				}
			}
			// line 381
			return e;
		}
		bool Try_Scan_ComplexNameDecl(int lookaheadAmt, bool thisAllowed = false)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_ComplexNameDecl(thisAllowed);
		}
		bool Scan_ComplexNameDecl(bool thisAllowed = false)
		{
			TokenType la0, la1;
			// Line 367: (ComplexThisDecl | IdAtom (TT.ColonColon IdAtom)? (TParamsDecl)? (TT.Dot IdAtom (TParamsDecl)?)* (TT.Dot ComplexThisDecl)?)
			la0 = LA0;
			if (la0 == TT.@this)
				{if (!Scan_ComplexThisDecl(thisAllowed))
					return false;}
			else {
				if (!Scan_IdAtom())
					return false;
				// Line 370: (TT.ColonColon IdAtom)?
				do {
					la0 = LA0;
					if (la0 == TT.ColonColon) {
						switch (LA(1)) {
						case TT.@operator:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							goto match1;
						case TT.ContextualKeyword:
							{
								if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
									goto match1;
							}
							break;
						}
					}
					break;
				match1:
					{
						if (!TryMatch((int) TT.ColonColon))
							return false;
						if (!Scan_IdAtom())
							return false;
					}
				} while (false);
				// Line 373: (TParamsDecl)?
				do {
					la0 = LA0;
					if (la0 == TT.LT) {
						switch (LA(1)) {
						case TT.@operator:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							goto matchTParamsDecl;
						case TT.ContextualKeyword:
							{
								if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
									goto matchTParamsDecl;
							}
							break;
						case TT.LBrack:
							{
								if (!(Down(1) && Up(Try_Scan_AsmOrModLabel(0))))
									goto matchTParamsDecl;
							}
							break;
						case TT.@in:
						case TT.AttrKeyword:
						case TT.GT:
							goto matchTParamsDecl;
						}
					} else if (la0 == TT.Dot) {
						la1 = LA(1);
						if (la1 == TT.LBrack)
							goto matchTParamsDecl;
					} else if (la0 == TT.Not) {
						la1 = LA(1);
						if (la1 == TT.LParen)
							goto matchTParamsDecl;
					}
					break;
				matchTParamsDecl:
					{
						if (!Scan_TParamsDecl())
							return false;
					}
				} while (false);
				// Line 374: (TT.Dot IdAtom (TParamsDecl)?)*
				for (;;) {
					la0 = LA0;
					if (la0 == TT.Dot) {
						switch (LA(1)) {
						case TT.@operator:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							goto match1_a;
						case TT.ContextualKeyword:
							{
								if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
									goto match1_a;
								else
									goto stop;
							}
						default:
							goto stop;
						}
					} else
						goto stop;
				match1_a:
					{
						if (!TryMatch((int) TT.Dot))
							return false;
						if (!Scan_IdAtom())
							return false;
						// Line 376: (TParamsDecl)?
						do {
							la0 = LA0;
							if (la0 == TT.LT) {
								switch (LA(1)) {
								case TT.@operator:
								case TT.Id:
								case TT.Substitute:
								case TT.TypeKeyword:
									goto matchTParamsDecl_a;
								case TT.ContextualKeyword:
									{
										if (!(_insideLinqExpr && LinqKeywords.Contains(LT(1).Value)))
											goto matchTParamsDecl_a;
									}
									break;
								case TT.LBrack:
									{
										if (!(Down(1) && Up(Try_Scan_AsmOrModLabel(0))))
											goto matchTParamsDecl_a;
									}
									break;
								case TT.@in:
								case TT.AttrKeyword:
								case TT.GT:
									goto matchTParamsDecl_a;
								}
							} else if (la0 == TT.Dot) {
								la1 = LA(1);
								if (la1 == TT.LBrack)
									goto matchTParamsDecl_a;
							} else if (la0 == TT.Not) {
								la1 = LA(1);
								if (la1 == TT.LParen)
									goto matchTParamsDecl_a;
							}
							break;
						matchTParamsDecl_a:
							{
								if (!Scan_TParamsDecl())
									return false;
							}
						} while (false);
					}
				}
			stop:;
				// Line 378: (TT.Dot ComplexThisDecl)?
				la0 = LA0;
				if (la0 == TT.Dot) {
					la1 = LA(1);
					if (la1 == TT.@this) {
						if (!TryMatch((int) TT.Dot))
							return false;
						if (!Scan_ComplexThisDecl(thisAllowed))
							return false;
					}
				}
			}
			return true;
		}
		LNode ComplexThisDecl(bool allowed)
		{
			TokenType la0;
			LNode result = default(LNode);
			// line 385
			if ((!allowed)) {
				Error("'this' is not allowed in this location.");
			}
			var t = Match((int) TT.@this);
			// line 386
			result = F.Id(t);
			// Line 387: (TParamsDecl)?
			la0 = LA0;
			if (la0 == TT.Dot || la0 == TT.LT || la0 == TT.Not) {
				switch (LA(1)) {
				case TT.@in:
				case TT.@operator:
				case TT.AttrKeyword:
				case TT.ContextualKeyword:
				case TT.GT:
				case TT.Id:
				case TT.LBrack:
				case TT.LParen:
				case TT.Substitute:
				case TT.TypeKeyword:
					TParamsDecl(ref result);
					break;
				}
			}
			return result;
		}
		bool Scan_ComplexThisDecl(bool allowed)
		{
			TokenType la0;
			if (!TryMatch((int) TT.@this))
				return false;
			// Line 387: (TParamsDecl)?
			la0 = LA0;
			if (la0 == TT.Dot || la0 == TT.LT || la0 == TT.Not) {
				switch (LA(1)) {
				case TT.@in:
				case TT.@operator:
				case TT.AttrKeyword:
				case TT.ContextualKeyword:
				case TT.GT:
				case TT.Id:
				case TT.LBrack:
				case TT.LParen:
				case TT.Substitute:
				case TT.TypeKeyword:
					if (!Scan_TParamsDecl())
						return false;
					break;
				}
			}
			return true;
		}
		void TParamsDecl(ref LNode r)
		{
			TokenType la0;
			// line 393
			WList<LNode> list = new WList<LNode> { 
				r
			};
			Token end;
			bool complex;
			// Line 395: ( TT.LT (TParamDecl (TT.Comma TParamDecl)*)? TT.GT | TT.Dot TT.LBrack TT.RBrack | TT.Not TT.LParen TT.RParen )
			la0 = LA0;
			if (la0 == TT.LT) {
				Skip();
				// Line 395: (TParamDecl (TT.Comma TParamDecl)*)?
				switch (LA0) {
				case TT.@in:
				case TT.@operator:
				case TT.AttrKeyword:
				case TT.ContextualKeyword:
				case TT.Id:
				case TT.LBrack:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						list.Add(TParamDecl());
						// Line 395: (TT.Comma TParamDecl)*
						for (;;) {
							la0 = LA0;
							if (la0 == TT.Comma) {
								Skip();
								list.Add(TParamDecl());
							} else
								break;
						}
					}
					break;
				}
				end = Match((int) TT.GT);
			} else if (la0 == TT.Dot) {
				Skip();
				var t = Match((int) TT.LBrack);
				end = Match((int) TT.RBrack);
				// line 396
				list = AppendExprsInside(t, list);
			} else {
				Match((int) TT.Not);
				var t = Match((int) TT.LParen);
				end = Match((int) TT.RParen);
				// line 397
				list = AppendExprsInside(t, list);
			}
			// line 400
			int start = r.Range.StartIndex;
			r = F.Call(S.Of, list.ToVList(), start, end.EndIndex);
		}
		bool Try_Scan_TParamsDecl(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_TParamsDecl();
		}
		bool Scan_TParamsDecl()
		{
			TokenType la0;
			// Line 395: ( TT.LT (TParamDecl (TT.Comma TParamDecl)*)? TT.GT | TT.Dot TT.LBrack TT.RBrack | TT.Not TT.LParen TT.RParen )
			la0 = LA0;
			if (la0 == TT.LT) {
				if (!TryMatch((int) TT.LT))
					return false;
				// Line 395: (TParamDecl (TT.Comma TParamDecl)*)?
				switch (LA0) {
				case TT.@in:
				case TT.@operator:
				case TT.AttrKeyword:
				case TT.ContextualKeyword:
				case TT.Id:
				case TT.LBrack:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						if (!Scan_TParamDecl())
							return false;
						// Line 395: (TT.Comma TParamDecl)*
						for (;;) {
							la0 = LA0;
							if (la0 == TT.Comma) {
								if (!TryMatch((int) TT.Comma))
									return false;
								if (!Scan_TParamDecl())
									return false;
							} else
								break;
						}
					}
					break;
				}
				if (!TryMatch((int) TT.GT))
					return false;
			} else if (la0 == TT.Dot) {
				if (!TryMatch((int) TT.Dot))
					return false;
				if (!TryMatch((int) TT.LBrack))
					return false;
				if (!TryMatch((int) TT.RBrack))
					return false;
			} else {
				if (!TryMatch((int) TT.Not))
					return false;
				if (!TryMatch((int) TT.LParen))
					return false;
				if (!TryMatch((int) TT.RParen))
					return false;
			}
			return true;
		}
		LNode TParamDecl()
		{
			LNode result = default(LNode);
			WList<LNode> attrs = null;
			int startIndex = GetTextPosition(InputPosition);
			// Line 407: (ComplexId / NormalAttributes TParamAttributeKeywords IdAtom)
			switch (LA0) {
			case TT.@operator:
			case TT.ContextualKeyword:
			case TT.Id:
			case TT.Substitute:
			case TT.TypeKeyword:
				result = ComplexId();
				break;
			default:
				{
					NormalAttributes(ref attrs);
					TParamAttributeKeywords(ref attrs);
					result = IdAtom();
				}
				break;
			}
			if ((attrs != null)) {
				result = result.WithAttrs(attrs.ToVList());
			}
			return result;
		}
		bool Scan_TParamDecl()
		{
			// Line 407: (ComplexId / NormalAttributes TParamAttributeKeywords IdAtom)
			switch (LA0) {
			case TT.@operator:
			case TT.ContextualKeyword:
			case TT.Id:
			case TT.Substitute:
			case TT.TypeKeyword:
				if (!Scan_ComplexId())
					return false;
				break;
			default:
				{
					if (!Scan_NormalAttributes())
						return false;
					if (!Scan_TParamAttributeKeywords())
						return false;
					if (!Scan_IdAtom())
						return false;
				}
				break;
			}
			return true;
		}
		LNode Atom()
		{
			TokenType la0, la1;
			// line 482
			LNode r;
			// Line 483: ( (TT.Dot|TT.Substitute) Atom | TT.@operator AnyOperator | (@`.`(TT, noMacro(@base))|@`.`(TT, noMacro(@this))|TT.Id|TT.TypeKeyword) | UnusualId | TT.Literal | ExprInParensAuto | BracedBlock | NewExpr | TokenLiteral | (TT.@checked|TT.@unchecked) TT.LParen TT.RParen | (@`.`(TT, noMacro(@default))|TT.@sizeof|TT.@typeof) TT.LParen TT.RParen | TT.@delegate TT.LParen TT.RParen TT.LBrace TT.RBrace | TT.@is DataType )
			switch (LA0) {
			case TT.Dot:
			case TT.Substitute:
				{
					var t = MatchAny();
					var e = Atom();
					e = AutoRemoveParens(e);
					r = F.Call((Symbol) t.Value, e, t.StartIndex, e.Range.EndIndex);
				}
				break;
			case TT.@operator:
				{
					var op = MatchAny();
					var t = AnyOperator();
					// line 486
					r = F.Attr(_triviaUseOperatorKeyword, F.Id((Symbol) t.Value, op.StartIndex, t.EndIndex));
				}
				break;
			case TT.@base:
			case TT.@this:
			case TT.Id:
			case TT.TypeKeyword:
				{
					var t = MatchAny();
					// line 488
					r = F.Id(t);
				}
				break;
			case TT.ContextualKeyword:
				{
					var t = UnusualId();
					// line 490
					r = F.Id(t);
				}
				break;
			case TT.Literal:
				{
					var t = MatchAny();
					// line 492
					r = F.Literal(t.Value, t.StartIndex, t.EndIndex);
				}
				break;
			case TT.LParen:
				r = ExprInParensAuto();
				break;
			case TT.LBrace:
				r = BracedBlock();
				break;
			case TT.@new:
				r = NewExpr();
				break;
			case TT.At:
				r = TokenLiteral();
				break;
			case TT.@checked:
			case TT.@unchecked:
				{
					var t = MatchAny();
					var args = Match((int) TT.LParen);
					var rp = Match((int) TT.RParen);
					// line 499
					r = F.Call((Symbol) t.Value, ExprListInside(args), t.StartIndex, rp.EndIndex);
				}
				break;
			case TT.@default:
			case TT.@sizeof:
			case TT.@typeof:
				{
					var t = MatchAny();
					var args = Match((int) TT.LParen);
					var rp = Match((int) TT.RParen);
					// line 502
					r = F.Call((Symbol) t.Value, TypeInside(args), t.StartIndex, rp.EndIndex);
				}
				break;
			case TT.@delegate:
				{
					var t = MatchAny();
					var args = Match((int) TT.LParen);
					Match((int) TT.RParen);
					var block = Match((int) TT.LBrace);
					var rb = Match((int) TT.RBrace);
					// line 504
					r = F.Call(S.Lambda, F.List(ExprListInside(args, false, true).ToVList()), F.Braces(StmtListInside(block).ToVList(), block.StartIndex, rb.EndIndex), t.StartIndex, rb.EndIndex);
				}
				break;
			case TT.@is:
				{
					var t = MatchAny();
					var dt = DataType();
					// line 506
					r = F.Call(S.Is, dt, t.StartIndex, dt.Range.EndIndex);
				}
				break;
			default:
				{
					// line 507
					r = Error("Invalid expression. Expected (parentheses), {braces}, identifier, literal, or $substitution.");
					// Line 507: greedy(~(EOF|TT.Comma|TT.Semicolon))*
					for (;;) {
						la0 = LA0;
						if (!(la0 == (TokenType) EOF || la0 == TT.Comma || la0 == TT.Semicolon)) {
							la1 = LA(1);
							if (la1 != (TokenType) EOF)
								Skip();
							else
								break;
						} else
							break;
					}
				}
				break;
			}
			// line 509
			return r;
		}
		bool Scan_Atom()
		{
			TokenType la0, la1;
			// Line 483: ( (TT.Dot|TT.Substitute) Atom | TT.@operator AnyOperator | (@`.`(TT, noMacro(@base))|@`.`(TT, noMacro(@this))|TT.Id|TT.TypeKeyword) | UnusualId | TT.Literal | ExprInParensAuto | BracedBlock | NewExpr | TokenLiteral | (TT.@checked|TT.@unchecked) TT.LParen TT.RParen | (@`.`(TT, noMacro(@default))|TT.@sizeof|TT.@typeof) TT.LParen TT.RParen | TT.@delegate TT.LParen TT.RParen TT.LBrace TT.RBrace | TT.@is DataType )
			switch (LA0) {
			case TT.Dot:
			case TT.Substitute:
				{
					if (!TryMatch((int) TT.Dot, (int) TT.Substitute))
						return false;
					if (!Scan_Atom())
						return false;
				}
				break;
			case TT.@operator:
				{
					if (!TryMatch((int) TT.@operator))
						return false;
					if (!Scan_AnyOperator())
						return false;
				}
				break;
			case TT.@base:
			case TT.@this:
			case TT.Id:
			case TT.TypeKeyword:
				if (!TryMatch((int) TT.@base, (int) TT.@this, (int) TT.Id, (int) TT.TypeKeyword))
					return false;
				break;
			case TT.ContextualKeyword:
				if (!Scan_UnusualId())
					return false;
				break;
			case TT.Literal:
				if (!TryMatch((int) TT.Literal))
					return false;
				break;
			case TT.LParen:
				if (!Scan_ExprInParensAuto())
					return false;
				break;
			case TT.LBrace:
				if (!Scan_BracedBlock())
					return false;
				break;
			case TT.@new:
				if (!Scan_NewExpr())
					return false;
				break;
			case TT.At:
				if (!Scan_TokenLiteral())
					return false;
				break;
			case TT.@checked:
			case TT.@unchecked:
				{
					if (!TryMatch((int) TT.@checked, (int) TT.@unchecked))
						return false;
					if (!TryMatch((int) TT.LParen))
						return false;
					if (!TryMatch((int) TT.RParen))
						return false;
				}
				break;
			case TT.@default:
			case TT.@sizeof:
			case TT.@typeof:
				{
					if (!TryMatch((int) TT.@default, (int) TT.@sizeof, (int) TT.@typeof))
						return false;
					if (!TryMatch((int) TT.LParen))
						return false;
					if (!TryMatch((int) TT.RParen))
						return false;
				}
				break;
			case TT.@delegate:
				{
					if (!TryMatch((int) TT.@delegate))
						return false;
					if (!TryMatch((int) TT.LParen))
						return false;
					if (!TryMatch((int) TT.RParen))
						return false;
					if (!TryMatch((int) TT.LBrace))
						return false;
					if (!TryMatch((int) TT.RBrace))
						return false;
				}
				break;
			case TT.@is:
				{
					if (!TryMatch((int) TT.@is))
						return false;
					if (!Scan_DataType())
						return false;
				}
				break;
			default:
				{
					// Line 507: greedy(~(EOF|TT.Comma|TT.Semicolon))*
					for (;;) {
						la0 = LA0;
						if (!(la0 == (TokenType) EOF || la0 == TT.Comma || la0 == TT.Semicolon)) {
							la1 = LA(1);
							if (la1 != (TokenType) EOF)
								{if (!TryMatchExcept((int) TT.Comma, (int) TT.Semicolon))
									return false;}
							else
								break;
						} else
							break;
					}
				}
				break;
			}
			return true;
		}
		static readonly HashSet<int> AnyOperator_set0 = NewSet((int) TT.Add, (int) TT.And, (int) TT.AndBits, (int) TT.At, (int) TT.Backslash, (int) TT.BQString, (int) TT.Colon, (int) TT.ColonColon, (int) TT.CompoundSet, (int) TT.DivMod, (int) TT.Dot, (int) TT.DotDot, (int) TT.EqNeq, (int) TT.Forward, (int) TT.GT, (int) TT.IncDec, (int) TT.LambdaArrow, (int) TT.LEGE, (int) TT.LT, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.NullCoalesce, (int) TT.NullDot, (int) TT.OrBits, (int) TT.OrXor, (int) TT.Power, (int) TT.PtrArrow, (int) TT.QuestionMark, (int) TT.QuickBind, (int) TT.QuickBindSet, (int) TT.Set, (int) TT.Sub, (int) TT.Substitute, (int) TT.XorBits);
		Token AnyOperator()
		{
			var op = Match(AnyOperator_set0);
			// line 519
			return op;
		}
		bool Scan_AnyOperator()
		{
			if (!TryMatch(AnyOperator_set0))
				return false;
			return true;
		}
		LNode NewExpr()
		{
			TokenType la0, la1;
			// line 524
			Token? majorDimension = null;
			int endIndex;
			var list = new WList<LNode>();
			var op = Match((int) TT.@new);
			// Line 530: ( &{(count = CountDims(LT($LI), @false)) > 0} TT.LBrack TT.RBrack TT.LBrace TT.RBrace | TT.LBrace TT.RBrace | DataType (TT.LParen TT.RParen (TT.LBrace TT.RBrace)? / (TT.LBrace TT.RBrace)?) )
			la0 = LA0;
			if (la0 == TT.LBrack) {
				Check((count = CountDims(LT(0), false)) > 0, "(count = CountDims(LT($LI), @false)) > 0");
				var lb = MatchAny();
				var rb = Match((int) TT.RBrack);
				// line 532
				var type = F.Id(S.GetArrayKeyword(count), lb.StartIndex, rb.EndIndex);
				lb = Match((int) TT.LBrace);
				rb = Match((int) TT.RBrace);
				// line 535
				list.Add(LNode.Call(type, type.Range));
				AppendInitializersInside(lb, list);
				endIndex = rb.EndIndex;
			} else if (la0 == TT.LBrace) {
				var lb = MatchAny();
				var rb = Match((int) TT.RBrace);
				// line 542
				list.Add(F.Missing);
				AppendInitializersInside(lb, list);
				endIndex = rb.EndIndex;
			} else {
				var type = DataType(false, out majorDimension);
				// Line 554: (TT.LParen TT.RParen (TT.LBrace TT.RBrace)? / (TT.LBrace TT.RBrace)?)
				do {
					la0 = LA0;
					if (la0 == TT.LParen) {
						la1 = LA(1);
						if (la1 == TT.RParen) {
							var lp = MatchAny();
							var rp = MatchAny();
							// line 556
							if ((majorDimension != null)) {
								Error("Syntax error: unexpected constructor argument list (...)");
							}
							list.Add(F.Call(type, ExprListInside(lp).ToVList(), type.Range.StartIndex, rp.EndIndex));
							endIndex = rp.EndIndex;
							// Line 562: (TT.LBrace TT.RBrace)?
							la0 = LA0;
							if (la0 == TT.LBrace) {
								la1 = LA(1);
								if (la1 == TT.RBrace) {
									var lb = MatchAny();
									var rb = MatchAny();
									// line 564
									AppendInitializersInside(lb, list);
									endIndex = rb.EndIndex;
								}
							}
						} else
							goto match2;
					} else
						goto match2;
					break;
				match2:
					{
						// line 571
						Token lb = op, rb = op;
						bool haveBraces = false;
						// Line 572: (TT.LBrace TT.RBrace)?
						la0 = LA0;
						if (la0 == TT.LBrace) {
							la1 = LA(1);
							if (la1 == TT.RBrace) {
								lb = MatchAny();
								rb = MatchAny();
								// line 572
								haveBraces = true;
							}
						}
						// line 574
						if ((majorDimension != null)) {
							list.Add(LNode.Call(type, ExprListInside(majorDimension.Value).ToVList(), type.Range));
						} else {
							list.Add(LNode.Call(type, type.Range));
						}
						if ((haveBraces)) {
							AppendInitializersInside(lb, list);
							endIndex = rb.EndIndex;
						} else {
							endIndex = type.Range.EndIndex;
						}
						if ((!haveBraces && majorDimension == null)) {
							if (IsArrayType(type)) {
								Error("Syntax error: missing array size expression");
							} else {
								Error("Syntax error: expected constructor argument list (...) or initializers {...}");
							}
						}
					}
				} while (false);
			}
			// line 595
			return F.Call(S.New, list.ToVList(), op.StartIndex, endIndex);
		}
		bool Scan_NewExpr()
		{
			TokenType la0, la1;
			if (!TryMatch((int) TT.@new))
				return false;
			// Line 530: ( &{(count = CountDims(LT($LI), @false)) > 0} TT.LBrack TT.RBrack TT.LBrace TT.RBrace | TT.LBrace TT.RBrace | DataType (TT.LParen TT.RParen (TT.LBrace TT.RBrace)? / (TT.LBrace TT.RBrace)?) )
			la0 = LA0;
			if (la0 == TT.LBrack) {
				if (!((count = CountDims(LT(0), false)) > 0))
					return false;
				if (!TryMatch((int) TT.LBrack))
					return false;
				if (!TryMatch((int) TT.RBrack))
					return false;
				if (!TryMatch((int) TT.LBrace))
					return false;
				if (!TryMatch((int) TT.RBrace))
					return false;
			} else if (la0 == TT.LBrace) {
				if (!TryMatch((int) TT.LBrace))
					return false;
				if (!TryMatch((int) TT.RBrace))
					return false;
			} else {
				if (!Scan_DataType(false))
					return false;
				// Line 554: (TT.LParen TT.RParen (TT.LBrace TT.RBrace)? / (TT.LBrace TT.RBrace)?)
				do {
					la0 = LA0;
					if (la0 == TT.LParen) {
						la1 = LA(1);
						if (la1 == TT.RParen) {
							if (!TryMatch((int) TT.LParen))
								return false;
							if (!TryMatch((int) TT.RParen))
								return false;
							// Line 562: (TT.LBrace TT.RBrace)?
							la0 = LA0;
							if (la0 == TT.LBrace) {
								la1 = LA(1);
								if (la1 == TT.RBrace) {
									if (!TryMatch((int) TT.LBrace))
										return false;
									if (!TryMatch((int) TT.RBrace))
										return false;
								}
							}
						} else
							goto match2;
					} else
						goto match2;
					break;
				match2:
					{
						// Line 572: (TT.LBrace TT.RBrace)?
						la0 = LA0;
						if (la0 == TT.LBrace) {
							la1 = LA(1);
							if (la1 == TT.RBrace) {
								if (!TryMatch((int) TT.LBrace))
									return false;
								if (!TryMatch((int) TT.RBrace))
									return false;
							}
						}
					}
				} while (false);
			}
			return true;
		}
		LNode ExprInParensAuto()
		{
			// Line 609: (&(ExprInParens (TT.LambdaArrow|TT.Set)) ExprInParens / ExprInParens)
			if (Try_ExprInParensAuto_Test0(0)) {
				var r = ExprInParens(true);
				// line 610
				return r;
			} else {
				var r = ExprInParens(false);
				// line 611
				return r;
			}
		}
		bool Scan_ExprInParensAuto()
		{
			// Line 609: (&(ExprInParens (TT.LambdaArrow|TT.Set)) ExprInParens / ExprInParens)
			if (Try_ExprInParensAuto_Test0(0))
				{if (!Scan_ExprInParens(true))
					return false;}
			else if (!Scan_ExprInParens(false))
				return false;
			return true;
		}
		LNode TokenLiteral()
		{
			TokenType la0;
			Token at = default(Token);
			Token L = default(Token);
			Token R = default(Token);
			at = MatchAny();
			// Line 616: (TT.LBrack TT.RBrack | TT.LBrace TT.RBrace)
			la0 = LA0;
			if (la0 == TT.LBrack) {
				L = MatchAny();
				R = Match((int) TT.RBrack);
			} else {
				L = Match((int) TT.LBrace);
				R = Match((int) TT.RBrace);
			}
			// line 617
			return F.Literal(L.Children, at.StartIndex, R.EndIndex);
		}
		bool Scan_TokenLiteral()
		{
			TokenType la0;
			if (!TryMatch((int) TT.At))
				return false;
			// Line 616: (TT.LBrack TT.RBrack | TT.LBrace TT.RBrace)
			la0 = LA0;
			if (la0 == TT.LBrack) {
				if (!TryMatch((int) TT.LBrack))
					return false;
				if (!TryMatch((int) TT.RBrack))
					return false;
			} else {
				if (!TryMatch((int) TT.LBrace))
					return false;
				if (!TryMatch((int) TT.RBrace))
					return false;
			}
			return true;
		}
		LNode PrimaryExpr()
		{
			TokenType la0;
			var e = Atom();
			FinishPrimaryExpr(ref e);
			// Line 624: (TT.NullDot PrimaryExpr)?
			la0 = LA0;
			if (la0 == TT.NullDot) {
				Skip();
				var rhs = PrimaryExpr();
				// line 624
				e = F.Call(S.NullDot, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex);
			}
			// line 626
			return e;
		}
		void FinishPrimaryExpr(ref LNode e)
		{
			TokenType la1;
			// Line 631: greedy( (TT.ColonColon|TT.Dot|TT.PtrArrow|TT.QuickBind) Atom / PrimaryExpr_NewStyleCast / TT.LParen TT.RParen | TT.LBrack TT.RBrack | TT.QuestionMark TT.LBrack TT.RBrack | TT.IncDec | &(TParams ~(TT.ContextualKeyword|TT.Id)) ((TT.LT|TT.Not) | TT.Dot TT.LBrack) => TParams | BracedBlock )*
			for (;;) {
				switch (LA0) {
				case TT.Dot:
					{
						if (Try_FinishPrimaryExpr_Test0(0)) {
							switch (LA(1)) {
							case TT.@base:
							case TT.@default:
							case TT.@this:
							case TT.@checked:
							case TT.@delegate:
							case TT.@is:
							case TT.@new:
							case TT.@operator:
							case TT.@sizeof:
							case TT.@typeof:
							case TT.@unchecked:
							case TT.At:
							case TT.ContextualKeyword:
							case TT.Dot:
							case TT.Id:
							case TT.LBrace:
							case TT.Literal:
							case TT.LParen:
							case TT.Substitute:
							case TT.TypeKeyword:
								goto match1;
							default:
								TParams(ref e);
								break;
							}
						} else
							goto match1;
					}
					break;
				case TT.ColonColon:
				case TT.PtrArrow:
				case TT.QuickBind:
					goto match1;
				case TT.LParen:
					{
						if (Down(0) && Up(LA0 == TT.@as || LA0 == TT.@using || LA0 == TT.PtrArrow))
							e = PrimaryExpr_NewStyleCast(e);
						else {
							var lp = MatchAny();
							var rp = Match((int) TT.RParen);
							// line 635
							e = F.Call(e, ExprListInside(lp), e.Range.StartIndex, rp.EndIndex);
						}
					}
					break;
				case TT.LBrack:
					{
						var lb = MatchAny();
						var rb = Match((int) TT.RBrack);
						var list = new WList<LNode> { 
							e
						};
						e = F.Call(S.IndexBracks, AppendExprsInside(lb, list).ToVList(), e.Range.StartIndex, rb.EndIndex);
					}
					break;
				case TT.QuestionMark:
					{
						la1 = LA(1);
						if (la1 == TT.LBrack) {
							var t = MatchAny();
							var lb = MatchAny();
							var rb = Match((int) TT.RBrack);
							// line 653
							e = F.Call(S.NullIndexBracks, e, F.List(ExprListInside(lb).ToVList()), e.Range.StartIndex, rb.EndIndex);
						} else
							goto stop;
					}
					break;
				case TT.IncDec:
					{
						var t = MatchAny();
						// line 655
						e = F.Call(t.Value == S.PreInc ? S.PostInc : S.PostDec, e, e.Range.StartIndex, t.EndIndex);
					}
					break;
				case TT.LT:
					{
						if (Try_FinishPrimaryExpr_Test0(0))
							TParams(ref e);
						else
							goto stop;
					}
					break;
				case TT.Not:
					TParams(ref e);
					break;
				case TT.LBrace:
					{
						la1 = LA(1);
						if (la1 == TT.RBrace) {
							var bb = BracedBlock();
							// line 659
							if ((!e.IsCall || e.BaseStyle == NodeStyle.Operator)) {
								e = F.Call(e, bb, e.Range.StartIndex, bb.Range.EndIndex);
							} else {
								e = e.WithArgs(e.Args.Add(bb)).WithRange(e.Range.StartIndex, bb.Range.EndIndex);
							}
						} else
							goto stop;
					}
					break;
				default:
					goto stop;
				}
				continue;
			match1:
				{
					var op = MatchAny();
					var rhs = Atom();
					// line 632
					e = F.Call((Symbol) op.Value, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex);
				}
			}
		stop:;
		}
		LNode PrimaryExpr_NewStyleCast(LNode e)
		{
			TokenType la0;
			var lp = MatchAny();
			var rp = Match((int) TT.RParen);
			Down(lp);
			Symbol kind;
			WList<LNode> attrs = null;
			// Line 675: ( TT.PtrArrow | TT.@as | TT.@using )
			la0 = LA0;
			if (la0 == TT.PtrArrow) {
				Skip();
				// line 675
				kind = S.Cast;
			} else if (la0 == TT.@as) {
				Skip();
				// line 676
				kind = S.As;
			} else {
				Match((int) TT.@using);
				// line 677
				kind = S.UsingCast;
			}
			NormalAttributes(ref attrs);
			AttributeKeywords(ref attrs);
			var type = DataType();
			Match((int) EOF);
			// line 682
			if (attrs != null) {
				type = type.PlusAttrs(attrs.ToVList());
			}
			return Up(SetAlternateStyle(SetOperatorStyle(F.Call(kind, e, type, e.Range.StartIndex, rp.EndIndex))));
		}
		static readonly HashSet<int> PrefixExpr_set0 = NewSet((int) TT.@base, (int) TT.@default, (int) TT.@this, (int) TT.@checked, (int) TT.@delegate, (int) TT.@is, (int) TT.@new, (int) TT.@operator, (int) TT.@sizeof, (int) TT.@typeof, (int) TT.@unchecked, (int) TT.Add, (int) TT.AndBits, (int) TT.At, (int) TT.ContextualKeyword, (int) TT.Dot, (int) TT.DotDot, (int) TT.Forward, (int) TT.Id, (int) TT.IncDec, (int) TT.LBrace, (int) TT.Literal, (int) TT.LParen, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.Power, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword);
		LNode PrefixExpr()
		{
			TokenType la2;
			// Line 693: ( (TT.Add|TT.AndBits|TT.DotDot|TT.Forward|TT.IncDec|TT.Mul|TT.Not|TT.NotBits|TT.Sub) PrefixExpr | (&{Down($LI) && Up(Scan_DataType() && LA0 == EOF)} TT.LParen TT.RParen &!(((TT.Add|TT.BQString|TT.Dot|TT.Sub) | TT.IncDec TT.LParen)) PrefixExpr / TT.Power PrefixExpr / PrimaryExpr) )
			do {
				switch (LA0) {
				case TT.Add:
				case TT.AndBits:
				case TT.DotDot:
				case TT.Forward:
				case TT.IncDec:
				case TT.Mul:
				case TT.Not:
				case TT.NotBits:
				case TT.Sub:
					{
						var op = MatchAny();
						var e = PrefixExpr();
						// line 694
						return SetOperatorStyle(F.Call((Symbol) op.Value, e, op.StartIndex, e.Range.EndIndex));
					}
					break;
				case TT.LParen:
					{
						if (Down(0) && Up(Scan_DataType() && LA0 == EOF)) {
							la2 = LA(2);
							if (PrefixExpr_set0.Contains((int) la2)) {
								if (!Try_PrefixExpr_Test0(2)) {
									var lp = MatchAny();
									Match((int) TT.RParen);
									var e = PrefixExpr();
									// line 700
									Down(lp);
									return SetOperatorStyle(F.Call(S.Cast, e, Up(DataType()), lp.StartIndex, e.Range.EndIndex));
								} else
									goto matchPrimaryExpr;
							} else
								goto matchPrimaryExpr;
						} else
							goto matchPrimaryExpr;
					}
					break;
				case TT.Power:
					{
						var op = MatchAny();
						var e = PrefixExpr();
						// line 703
						return SetOperatorStyle(F.Call(S._Dereference, SetOperatorStyle(F.Call(S._Dereference, e, op.StartIndex + 1, e.Range.EndIndex)), op.StartIndex, e.Range.EndIndex));
					}
					break;
				default:
					goto matchPrimaryExpr;
				}
				break;
			matchPrimaryExpr:
				{
					var e = PrimaryExpr();
					// line 711
					return e;
				}
			} while (false);
		}
		LNode Expr(Precedence context)
		{
			TokenType la0, la1;
			Debug.Assert(context.CanParse(EP.Prefix));
			Precedence prec;
			var e = PrefixExpr();
			// Line 752: greedy( &{context.CanParse(prec = InfixPrecedenceOf($LA))} (TT.@in|TT.Add|TT.And|TT.AndBits|TT.BQString|TT.CompoundSet|TT.DivMod|TT.DotDot|TT.EqNeq|TT.GT|TT.LambdaArrow|TT.LEGE|TT.LT|TT.Mul|TT.NotBits|TT.NullCoalesce|TT.OrBits|TT.OrXor|TT.Power|TT.Set|TT.Sub|TT.XorBits) Expr | &{context.CanParse(prec = InfixPrecedenceOf($LA))} (TT.@as|TT.@is|TT.@using) DataType FinishPrimaryExpr | &{context.CanParse(EP.Shift)} &{LT($LI).EndIndex == LT($LI + 1).StartIndex} (TT.LT TT.LT Expr | TT.GT TT.GT Expr) | &{context.CanParse(EP.IfElse)} TT.QuestionMark Expr TT.Colon Expr )*
			for (;;) {
				switch (LA0) {
				case TT.GT:
				case TT.LT:
					{
						la0 = LA0;
						if (context.CanParse(prec = InfixPrecedenceOf(la0))) {
							if (LT(0).EndIndex == LT(0 + 1).StartIndex) {
								if (context.CanParse(EP.Shift)) {
									la1 = LA(1);
									if (PrefixExpr_set0.Contains((int) la1))
										goto match1;
									else if (la1 == TT.GT || la1 == TT.LT)
										goto match3;
									else
										goto stop;
								} else {
									la1 = LA(1);
									if (PrefixExpr_set0.Contains((int) la1))
										goto match1;
									else
										goto stop;
								}
							} else {
								la1 = LA(1);
								if (PrefixExpr_set0.Contains((int) la1))
									goto match1;
								else
									goto stop;
							}
						} else if (LT(0).EndIndex == LT(0 + 1).StartIndex) {
							if (context.CanParse(EP.Shift)) {
								la1 = LA(1);
								if (la1 == TT.GT || la1 == TT.LT)
									goto match3;
								else
									goto stop;
							} else
								goto stop;
						} else
							goto stop;
					}
				case TT.@in:
				case TT.Add:
				case TT.And:
				case TT.AndBits:
				case TT.BQString:
				case TT.CompoundSet:
				case TT.DivMod:
				case TT.DotDot:
				case TT.EqNeq:
				case TT.LambdaArrow:
				case TT.LEGE:
				case TT.Mul:
				case TT.NotBits:
				case TT.NullCoalesce:
				case TT.OrBits:
				case TT.OrXor:
				case TT.Power:
				case TT.Set:
				case TT.Sub:
				case TT.XorBits:
					{
						la0 = LA0;
						if (context.CanParse(prec = InfixPrecedenceOf(la0))) {
							la1 = LA(1);
							if (PrefixExpr_set0.Contains((int) la1))
								goto match1;
							else
								goto stop;
						} else
							goto stop;
					}
				case TT.@as:
				case TT.@is:
				case TT.@using:
					{
						la0 = LA0;
						if (context.CanParse(prec = InfixPrecedenceOf(la0))) {
							switch (LA(1)) {
							case TT.@operator:
							case TT.ContextualKeyword:
							case TT.Id:
							case TT.Substitute:
							case TT.TypeKeyword:
								{
									var op = MatchAny();
									var rhs = DataType(true);
									var opSym = op.Type() == TT.@using ? S.UsingCast : ((Symbol) op.Value);
									e = SetOperatorStyle(F.Call(opSym, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex));
									FinishPrimaryExpr(ref e);
								}
								break;
							default:
								goto stop;
							}
						} else
							goto stop;
					}
					break;
				case TT.QuestionMark:
					{
						if (context.CanParse(EP.IfElse)) {
							la1 = LA(1);
							if (PrefixExpr_set0.Contains((int) la1)) {
								Skip();
								var then = Expr(ContinueExpr);
								Match((int) TT.Colon);
								var @else = Expr(EP.IfElse);
								// line 775
								e = SetOperatorStyle(F.Call(S.QuestionMark, e, then, @else, e.Range.StartIndex, @else.Range.EndIndex));
							} else
								goto stop;
						} else
							goto stop;
					}
					break;
				default:
					goto stop;
				}
				continue;
			match1:
				{
					var op = MatchAny();
					var rhs = Expr(prec);
					// line 756
					e = SetOperatorStyle(F.Call((Symbol) op.Value, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex));
				}
				continue;
			match3:
				{
					// Line 767: (TT.LT TT.LT Expr | TT.GT TT.GT Expr)
					la0 = LA0;
					if (la0 == TT.LT) {
						Skip();
						Match((int) TT.LT);
						var rhs = Expr(EP.Shift);
						// line 768
						e = SetOperatorStyle(F.Call(S.Shl, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex));
					} else {
						Match((int) TT.GT);
						Match((int) TT.GT);
						var rhs = Expr(EP.Shift);
						// line 770
						e = SetOperatorStyle(F.Call(S.Shr, e, rhs, e.Range.StartIndex, rhs.Range.EndIndex));
					}
				}
			}
		stop:;
			// line 777
			return e;
		}
		public LNode ExprStart(bool allowUnassignedVarDecl)
		{
			TokenType la0, la1;
			LNode result = default(LNode);
			// Line 789: ((TT.Id | UnusualId) TT.Colon ExprStart2 | ExprStart2)
			do {
				la0 = LA0;
				if (la0 == TT.ContextualKeyword || la0 == TT.Id) {
					if (Try_Scan_DetectVarDecl(0, allowUnassignedVarDecl)) {
						la1 = LA(1);
						if (la1 == TT.Colon)
							goto match1;
						else
							result = ExprStart2(allowUnassignedVarDecl);
					} else {
						la1 = LA(1);
						if (la1 == TT.Colon)
							goto match1;
						else
							result = ExprStart2(allowUnassignedVarDecl);
					}
				} else
					result = ExprStart2(allowUnassignedVarDecl);
				break;
			match1:
				{
					// line 789
					Token argName = default(Token);
					// Line 790: (TT.Id | UnusualId)
					la0 = LA0;
					if (la0 == TT.Id)
						argName = MatchAny();
					else
						argName = UnusualId();
					Skip();
					result = ExprStart2(allowUnassignedVarDecl);
					// line 792
					result = SetOperatorStyle(F.Call(S.NamedArg, F.Id(argName), result, argName.StartIndex, result.Range.EndIndex));
				}
			} while (false);
			return result;
		}
		public LNode ExprStart2(bool allowUnassignedVarDecl)
		{
			// line 797
			LNode e;
			WList<LNode> attrs = null;
			NormalAttributes(ref attrs);
			AttributeKeywords(ref attrs);
			var wc = WordAttributes(ref attrs);
			// Line 803: (&(DetectVarDecl) IdAtom => VarDeclExpr / Expr)
			do {
				switch (LA0) {
				case TT.@operator:
				case TT.ContextualKeyword:
				case TT.Id:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						if (Try_Scan_DetectVarDecl(0, allowUnassignedVarDecl))
							e = VarDeclExpr();
						else
							goto matchExpr;
					}
					break;
				default:
					goto matchExpr;
				}
				break;
			matchExpr:
				{
					// line 805
					if ((wc != 0)) {
						NonKeywordAttrError(attrs, "expression");
					}
					e = Expr(ContinueExpr);
				}
			} while (false);
			// line 809
			if ((attrs != null)) {
				e = e.PlusAttrs(attrs.ToVList());
			}
			return e;
		}
		void DetectVarDecl(bool allowUnassigned)
		{
			VarDeclStart();
			// Line 826: ( (TT.QuickBindSet|TT.Set) NoUnmatchedColon | &{allowUnassigned} (EOF|TT.Comma) | TT.LBrace )
			switch (LA0) {
			case TT.QuickBindSet:
			case TT.Set:
				{
					Skip();
					NoUnmatchedColon();
				}
				break;
			case EOF:
			case TT.Comma:
				{
					Check(allowUnassigned, "allowUnassigned");
					Skip();
				}
				break;
			default:
				Match((int) TT.LBrace);
				break;
			}
		}
		bool Try_Scan_DetectVarDecl(int lookaheadAmt, bool allowUnassigned)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_DetectVarDecl(allowUnassigned);
		}
		bool Scan_DetectVarDecl(bool allowUnassigned)
		{
			if (!Scan_VarDeclStart())
				return false;
			// Line 826: ( (TT.QuickBindSet|TT.Set) NoUnmatchedColon | &{allowUnassigned} (EOF|TT.Comma) | TT.LBrace )
			switch (LA0) {
			case TT.QuickBindSet:
			case TT.Set:
				{
					if (!TryMatch((int) TT.QuickBindSet, (int) TT.Set))
						return false;
					if (!Scan_NoUnmatchedColon())
						return false;
				}
				break;
			case EOF:
			case TT.Comma:
				{
					if (!allowUnassigned)
						return false;
					if (!TryMatch((int) EOF, (int) TT.Comma))
						return false;
				}
				break;
			default:
				if (!TryMatch((int) TT.LBrace))
					return false;
				break;
			}
			return true;
		}
		void NoUnmatchedColon()
		{
			TokenType la0;
			// Line 844: (SubConditional | ~(EOF|TT.Colon|TT.Comma|TT.QuestionMark|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.QuestionMark)
					SubConditional();
				else if (!(la0 == (TokenType) EOF || la0 == TT.Colon || la0 == TT.Comma || la0 == TT.QuestionMark || la0 == TT.Semicolon))
					Skip();
				else
					break;
			}
			Match((int) EOF, (int) TT.Comma, (int) TT.Semicolon);
		}
		bool Scan_NoUnmatchedColon()
		{
			TokenType la0;
			// Line 844: (SubConditional | ~(EOF|TT.Colon|TT.Comma|TT.QuestionMark|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.QuestionMark)
					{if (!Scan_SubConditional())
						return false;}
				else if (!(la0 == (TokenType) EOF || la0 == TT.Colon || la0 == TT.Comma || la0 == TT.QuestionMark || la0 == TT.Semicolon))
					{if (!TryMatchExcept((int) TT.Colon, (int) TT.Comma, (int) TT.QuestionMark, (int) TT.Semicolon))
						return false;}
				else
					break;
			}
			if (!TryMatch((int) EOF, (int) TT.Comma, (int) TT.Semicolon))
				return false;
			return true;
		}
		void SubConditional()
		{
			TokenType la0;
			Skip();
			// Line 849: nongreedy(SubConditional / ~(EOF|TT.Comma|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (la0 == (TokenType) EOF || la0 == TT.Colon)
					break;
				else if (la0 == TT.QuestionMark)
					SubConditional();
				else
					MatchExcept((int) TT.Comma, (int) TT.Semicolon);
			}
			Match((int) TT.Colon);
		}
		bool Scan_SubConditional()
		{
			TokenType la0;
			if (!TryMatch((int) TT.QuestionMark))
				return false;
			// Line 849: nongreedy(SubConditional / ~(EOF|TT.Comma|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (la0 == (TokenType) EOF || la0 == TT.Colon)
					break;
				else if (la0 == TT.QuestionMark)
					{if (!Scan_SubConditional())
						return false;}
				else if (!TryMatchExcept((int) TT.Comma, (int) TT.Semicolon))
					return false;
			}
			if (!TryMatch((int) TT.Colon))
				return false;
			return true;
		}
		LNode VarDeclExpr()
		{
			LNode result = default(LNode);
			var pair = VarDeclStart();
			// line 853
			LNode type = pair.Item1, name = pair.Item2;
			// Line 856: (RestOfPropertyDefinition / VarInitializerOpt)
			switch (LA0) {
			case TT.At:
			case TT.ContextualKeyword:
			case TT.Forward:
			case TT.LambdaArrow:
			case TT.LBrace:
			case TT.LBrack:
				result = RestOfPropertyDefinition(type, name, true);
				break;
			default:
				{
					name = VarInitializerOpt(name, IsArrayType(type));
					// line 858
					result = F.Call(S.Var, type, name, type.Range.StartIndex, name.Range.EndIndex);
				}
				break;
			}
			return result;
		}
		Pair<LNode,LNode> VarDeclStart()
		{
			var e = DataType();
			var id = IdAtom();
			MaybeRecognizeVarAsKeyword(ref e);
			return Pair.Create(e, id);
		}
		bool Scan_VarDeclStart()
		{
			if (!Scan_DataType())
				return false;
			if (!Scan_IdAtom())
				return false;
			return true;
		}
		LNode ExprInParens(bool allowUnassignedVarDecl)
		{
			var lp = Match((int) TT.LParen);
			var rp = Match((int) TT.RParen);
			// line 890
			if ((!Down(lp))) {
				return F.Call(S.Tuple, lp.StartIndex, rp.EndIndex);
			}
			return Up(InParens_ExprOrTuple(allowUnassignedVarDecl, lp.StartIndex, rp.EndIndex));
		}
		bool Scan_ExprInParens(bool allowUnassignedVarDecl)
		{
			if (!TryMatch((int) TT.LParen))
				return false;
			if (!TryMatch((int) TT.RParen))
				return false;
			return true;
		}
		static readonly HashSet<int> InParens_ExprOrTuple_set0 = NewSet((int) TT.@base, (int) TT.@default, (int) TT.@this, (int) TT.@checked, (int) TT.@delegate, (int) TT.@is, (int) TT.@new, (int) TT.@operator, (int) TT.@sizeof, (int) TT.@typeof, (int) TT.@unchecked, (int) TT.Add, (int) TT.AndBits, (int) TT.At, (int) TT.AttrKeyword, (int) TT.ContextualKeyword, (int) TT.Dot, (int) TT.DotDot, (int) TT.Forward, (int) TT.Id, (int) TT.IncDec, (int) TT.LBrace, (int) TT.LBrack, (int) TT.Literal, (int) TT.LParen, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.Power, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword);
		LNode InParens_ExprOrTuple(bool allowUnassignedVarDecl, int startIndex, int endIndex)
		{
			TokenType la0, la1;
			// Line 896: (ExprStart (TT.Comma (~(EOF))* => (TT.Comma ExprStart | TT.Comma)*)? | )
			la0 = LA0;
			if (InParens_ExprOrTuple_set0.Contains((int) la0)) {
				var e = ExprStart(allowUnassignedVarDecl);
				// Line 897: (TT.Comma (~(EOF))* => (TT.Comma ExprStart | TT.Comma)*)?
				la0 = LA0;
				if (la0 == TT.Comma) {
					// line 898
					var list = new VList<LNode> { 
						e
					};
					// Line 899: (TT.Comma ExprStart | TT.Comma)*
					for (;;) {
						la0 = LA0;
						if (la0 == TT.Comma) {
							la1 = LA(1);
							if (InParens_ExprOrTuple_set0.Contains((int) la1)) {
								Skip();
								list.Add(ExprStart(allowUnassignedVarDecl));
							} else
								Skip();
						} else
							break;
					}
					// line 902
					return F.Tuple(list, startIndex, endIndex);
				}
				// line 904
				return F.InParens(e, startIndex, endIndex);
			} else
				// line 906
				return F.Tuple(VList<LNode>.Empty, startIndex, endIndex);
			Match((int) EOF);
		}
		LNode BracedBlock(Symbol spaceName = null, Symbol target = null, int startIndex = -1)
		{
			Token lit_lcub = default(Token);
			Token lit_rcub = default(Token);
			// line 911
			var oldSpace = _spaceName;
			_spaceName = spaceName ?? oldSpace;
			lit_lcub = Match((int) TT.LBrace);
			lit_rcub = Match((int) TT.RBrace);
			// line 915
			if ((startIndex == -1)) {
				startIndex = lit_lcub.StartIndex;
			}
			var stmts = StmtListInside(lit_lcub).ToVList();
			_spaceName = oldSpace;
			return F.Call(target ?? S.Braces, stmts, lit_lcub.StartIndex, lit_rcub.EndIndex).SetBaseStyle(NodeStyle.Statement);
		}
		bool Scan_BracedBlock(Symbol spaceName = null, Symbol target = null, int startIndex = -1)
		{
			if (!TryMatch((int) TT.LBrace))
				return false;
			if (!TryMatch((int) TT.RBrace))
				return false;
			return true;
		}
		void NormalAttributes(ref WList<LNode> attrs)
		{
			TokenType la0, la1;
			// Line 928: (&!{Down($LI) && Up(Try_Scan_AsmOrModLabel(0))} TT.LBrack TT.RBrack)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.LBrack) {
					if (!(Down(0) && Up(Try_Scan_AsmOrModLabel(0)))) {
						la1 = LA(1);
						if (la1 == TT.RBrack) {
							var t = MatchAny();
							Skip();
							// line 931
							if ((Down(t))) {
								AttributeContents(ref attrs);
								Up();
							}
						} else
							break;
					} else
						break;
				} else
					break;
			}
		}
		bool Try_Scan_NormalAttributes(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_NormalAttributes();
		}
		bool Scan_NormalAttributes()
		{
			TokenType la0, la1;
			// Line 928: (&!{Down($LI) && Up(Try_Scan_AsmOrModLabel(0))} TT.LBrack TT.RBrack)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.LBrack) {
					if (!(Down(0) && Up(Try_Scan_AsmOrModLabel(0)))) {
						la1 = LA(1);
						if (la1 == TT.RBrack) {
							if (!TryMatch((int) TT.LBrack))
								return false;
							if (!TryMatch((int) TT.RBrack))
								return false;
						} else
							break;
					} else
						break;
				} else
					break;
			}
			return true;
		}
		void AttributeContents(ref WList<LNode> attrs)
		{
			TokenType la0, la1;
			// line 939
			Token attrTarget = default(Token);
			// Line 941: greedy((@`.`(TT, noMacro(@return))|TT.ContextualKeyword|TT.Id) TT.Colon)?
			la0 = LA0;
			if (la0 == TT.@return || la0 == TT.ContextualKeyword || la0 == TT.Id) {
				la1 = LA(1);
				if (la1 == TT.Colon) {
					attrTarget = MatchAny();
					Skip();
				}
			}
			ExprList(attrs = attrs ?? new WList<LNode>(), allowTrailingComma: true, allowUnassignedVarDecl: true);
			// line 946
			if (attrTarget.Value != null) {
				var attrTargetNode = F.Id(attrTarget);
				for (int i = 0; i < attrs.Count; i++) {
					var attr = attrs[i];
					if ((!IsNamedArg(attr))) {
						attrs[i] = SetOperatorStyle(F.Call(S.NamedArg, attrTargetNode, attr, attrTarget.StartIndex, attr.Range.EndIndex));
					} else {
						attrTargetNode = attrs[i].Args[1];
						Error(attrTargetNode, "Syntax error: only one attribute target is allowed");
					}
				}
			}
		}
		void AttributeKeywords(ref WList<LNode> attrs)
		{
			TokenType la0;
			// Line 965: (TT.AttrKeyword)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.AttrKeyword) {
					var t = MatchAny();
					// line 966
					(attrs = attrs ?? new WList<LNode>()).Add(F.Id(t));
				} else
					break;
			}
		}
		void TParamAttributeKeywords(ref WList<LNode> attrs)
		{
			TokenType la0;
			// Line 971: ((TT.@in|TT.AttrKeyword))*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.@in || la0 == TT.AttrKeyword) {
					var t = MatchAny();
					// line 972
					(attrs = attrs ?? new WList<LNode>()).Add(F.Id(t));
				} else
					break;
			}
		}
		bool Try_Scan_TParamAttributeKeywords(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_TParamAttributeKeywords();
		}
		bool Scan_TParamAttributeKeywords()
		{
			TokenType la0;
			// Line 971: ((TT.@in|TT.AttrKeyword))*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.@in || la0 == TT.AttrKeyword)
					{if (!TryMatch((int) TT.@in, (int) TT.AttrKeyword))
						return false;}
				else
					break;
			}
			return true;
		}
		int WordAttributes(ref WList<LNode> attrs)
		{
			TokenType la0;
			// line 1038
			TokenType LA1;
			int nonKeywords = 0;
			if (LA0 == TT.Id && ((LA1 = LA(1)) == TT.Set || LA1 == TT.LParen || LA1 == TT.Dot))
				 return 0;
			Token t;
			// Line 1044: (TT.AttrKeyword | ((@`.`(TT, noMacro(@this))|TT.@new|TT.Id) | UnusualId) &(( DataType ((TT.AttrKeyword|TT.Id|TT.TypeKeyword) | UnusualId) | @`.`(TT, noMacro(@this)) TT.LParen | (TT.@new|TT.AttrKeyword) | TT.@checked TT.LBrace TT.RBrace | TT.@unchecked TT.LBrace TT.RBrace | @`.`(TT, noMacro(@default)) TT.Colon | TT.@using TT.LParen | (@`.`(TT, noMacro(@break))|@`.`(TT, noMacro(@continue))|@`.`(TT, noMacro(@return))|@`.`(TT, noMacro(@throw))|TT.@case|TT.@class|TT.@delegate|TT.@do|TT.@enum|TT.@event|TT.@fixed|TT.@for|TT.@foreach|TT.@goto|TT.@interface|TT.@lock|TT.@namespace|TT.@struct|TT.@switch|TT.@try|TT.@while) )))*
			for (;;) {
				switch (LA0) {
				case TT.AttrKeyword:
					{
						t = MatchAny();
						// line 1044
						attrs.Add(F.Id(t));
					}
					break;
				case TT.@this:
				case TT.@new:
				case TT.ContextualKeyword:
				case TT.Id:
					{
						if (Try_WordAttributes_Test0(1)) {
							// Line 1045: ((@`.`(TT, noMacro(@this))|TT.@new|TT.Id) | UnusualId)
							la0 = LA0;
							if (la0 == TT.@this || la0 == TT.@new || la0 == TT.Id)
								t = MatchAny();
							else
								t = UnusualId();
							// line 1061
							LNode node;
							if ((t.Type() == TT.@new || t.Type() == TT.@this)) {
								node = F.Id(t);
							} else {
								node = F.Attr(_triviaWordAttribute, F.Id("#" + t.Value.ToString(), t.StartIndex, t.EndIndex));
							}
							attrs = attrs ?? new WList<LNode>();
							attrs.Add(node);
							nonKeywords++;
						} else
							goto stop;
					}
					break;
				default:
					goto stop;
				}
			}
		stop:;
			// line 1072
			return nonKeywords;
		}
		public LNode Stmt()
		{
			LNode result = default(LNode);
			_stmtAttrs.Clear();
			int startIndex = LT0.StartIndex;
			NormalAttributes(ref _stmtAttrs);
			AttributeKeywords(ref _stmtAttrs);
			// line 1129
			var oldPosition = InputPosition;
			int wordAttrCount;
			var cat = DetectStatementCategory(out wordAttrCount);
			for (; oldPosition < InputPosition; oldPosition++) {
				Token word = _tokens[oldPosition];
				LNode wordAttr;
				if ((word.Kind == TokenKind.AttrKeyword || word.Type() == TT.@new)) {
					wordAttr = F.Id(word);
				} else {
					wordAttr = F.Attr(_triviaWordAttribute, F.Id("#" + word.Value.ToString(), word.StartIndex, word.EndIndex));
				}
				_stmtAttrs.Add(wordAttr);
			}
			var attrs = _stmtAttrs.ToVList();
			switch ((cat)) {
			case StmtCat.MethodOrPropOrVar:
				result = MethodOrPropertyOrVarStmt(startIndex, attrs);
				break;
			case StmtCat.KeywordStmt:
				result = KeywordStmt(startIndex, attrs);
				break;
			case StmtCat.IdStmt:
				result = IdStmt(startIndex, attrs, wordAttrCount != 0);
				break;
			case StmtCat.OtherStmt:
				result = OtherStmt(startIndex, attrs, wordAttrCount != 0);
				break;
			case StmtCat.ThisConstructor:
				result = Constructor(startIndex, attrs);
				break;
			default:
				throw new Exception("Parser bug");
			}
			return result;
		}
		LNode MethodOrPropertyOrVarStmt(int startIndex, VList<LNode> attrs)
		{
			TokenType la0;
			LNode result = default(LNode);
			// Line 1170: ( TraitDecl / AliasDecl / MethodOrPropertyOrVar )
			do {
				la0 = LA0;
				if (la0 == TT.ContextualKeyword) {
					if (Is(0, _trait)) {
						if (Is(0, _alias)) {
							if (!(_insideLinqExpr && LinqKeywords.Contains(LT(0).Value))) {
								switch (LA(1)) {
								case TT.@this:
								case TT.@operator:
								case TT.ContextualKeyword:
								case TT.Id:
								case TT.Substitute:
								case TT.TypeKeyword:
									goto matchTraitDecl;
								default:
									result = MethodOrPropertyOrVar(startIndex, attrs);
									break;
								}
							} else
								goto matchTraitDecl;
						} else if (!(_insideLinqExpr && LinqKeywords.Contains(LT(0).Value))) {
							switch (LA(1)) {
							case TT.@this:
							case TT.@operator:
							case TT.ContextualKeyword:
							case TT.Id:
							case TT.Substitute:
							case TT.TypeKeyword:
								goto matchTraitDecl;
							default:
								result = MethodOrPropertyOrVar(startIndex, attrs);
								break;
							}
						} else
							goto matchTraitDecl;
					} else if (Is(0, _alias)) {
						if (!(_insideLinqExpr && LinqKeywords.Contains(LT(0).Value))) {
							switch (LA(1)) {
							case TT.@this:
							case TT.@operator:
							case TT.ContextualKeyword:
							case TT.Id:
							case TT.Substitute:
							case TT.TypeKeyword:
								goto matchAliasDecl;
							default:
								result = MethodOrPropertyOrVar(startIndex, attrs);
								break;
							}
						} else
							goto matchAliasDecl;
					} else
						result = MethodOrPropertyOrVar(startIndex, attrs);
				} else
					result = MethodOrPropertyOrVar(startIndex, attrs);
				break;
			matchTraitDecl:
				{
					result = TraitDecl(startIndex);
					// line 1170
					result = result.PlusAttrs(attrs);
				}
				break;
			matchAliasDecl:
				{
					result = AliasDecl(startIndex);
					// line 1171
					result = result.PlusAttrs(attrs);
				}
			} while (false);
			return result;
		}
		static readonly HashSet<int> KeywordStmt_set0 = NewSet((int) TT.@base, (int) TT.@default, (int) TT.@this, (int) TT.@checked, (int) TT.@delegate, (int) TT.@is, (int) TT.@new, (int) TT.@operator, (int) TT.@sizeof, (int) TT.@typeof, (int) TT.@unchecked, (int) TT.Add, (int) TT.AndBits, (int) TT.At, (int) TT.AttrKeyword, (int) TT.ContextualKeyword, (int) TT.Dot, (int) TT.DotDot, (int) TT.Forward, (int) TT.Id, (int) TT.IncDec, (int) TT.LBrace, (int) TT.LBrack, (int) TT.Literal, (int) TT.LParen, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.Power, (int) TT.Semicolon, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword);
		static readonly HashSet<int> KeywordStmt_set1 = NewSet((int) TT.@base, (int) TT.@default, (int) TT.@this, (int) TT.@checked, (int) TT.@delegate, (int) TT.@is, (int) TT.@new, (int) TT.@operator, (int) TT.@sizeof, (int) TT.@typeof, (int) TT.@unchecked, (int) TT.Add, (int) TT.AndBits, (int) TT.At, (int) TT.AttrKeyword, (int) TT.ContextualKeyword, (int) TT.Dot, (int) TT.DotDot, (int) TT.Forward, (int) TT.Id, (int) TT.IncDec, (int) TT.LBrace, (int) TT.LBrack, (int) TT.Literal, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.Power, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword);
		LNode KeywordStmt(int startIndex, VList<LNode> attrs)
		{
			TokenType la1;
			// line 1178
			LNode r;
			// Line 1179: ( (EventDecl | DelegateDecl | SpaceDecl | EnumDecl | CheckedOrUncheckedStmt | DoStmt | CaseStmt | GotoStmt | GotoCaseStmt | ReturnBreakContinueThrow | WhileStmt | ForStmt | ForEachStmt | SwitchStmt) | (UsingStmt / UsingDirective) | LockStmt | FixedStmt | TryStmt )
			do {
				switch (LA0) {
				case TT.@event:
					{
						r = EventDecl(startIndex);
						// line 1179
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@delegate:
					r = DelegateDecl(startIndex, attrs);
					break;
				case TT.@class:
				case TT.@interface:
				case TT.@namespace:
				case TT.@struct:
					{
						r = SpaceDecl(startIndex);
						// line 1181
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@enum:
					{
						r = EnumDecl(startIndex);
						// line 1182
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@checked:
				case TT.@unchecked:
					{
						r = CheckedOrUncheckedStmt(startIndex);
						// line 1183
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@do:
					{
						r = DoStmt(startIndex);
						// line 1184
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@case:
					{
						r = CaseStmt(startIndex);
						// line 1185
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@goto:
					{
						la1 = LA(1);
						if (KeywordStmt_set0.Contains((int) la1)) {
							r = GotoStmt(startIndex);
							// line 1186
							r = r.PlusAttrs(attrs);
						} else if (la1 == TT.@case) {
							r = GotoCaseStmt(startIndex);
							// line 1187
							r = r.PlusAttrs(attrs);
						} else
							goto error;
					}
					break;
				case TT.@break:
				case TT.@continue:
				case TT.@return:
				case TT.@throw:
					{
						r = ReturnBreakContinueThrow(startIndex);
						// line 1188
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@while:
					{
						r = WhileStmt(startIndex);
						// line 1189
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@for:
					{
						r = ForStmt(startIndex);
						// line 1190
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@foreach:
					{
						r = ForEachStmt(startIndex);
						// line 1191
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@switch:
					{
						r = SwitchStmt(startIndex);
						// line 1192
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@using:
					{
						la1 = LA(1);
						if (la1 == TT.LParen) {
							r = UsingStmt(startIndex);
							// line 1193
							r = r.PlusAttrs(attrs);
						} else if (KeywordStmt_set1.Contains((int) la1))
							r = UsingDirective(startIndex, attrs);
						else
							goto error;
					}
					break;
				case TT.@lock:
					{
						r = LockStmt(startIndex);
						// line 1195
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@fixed:
					{
						r = FixedStmt(startIndex);
						// line 1196
						r = r.PlusAttrs(attrs);
					}
					break;
				case TT.@try:
					{
						r = TryStmt(startIndex);
						// line 1197
						r = r.PlusAttrs(attrs);
					}
					break;
				default:
					goto error;
				}
				break;
			error:
				{
					// line 1198
					r = Error("Bug: Keyword statement expected at '{0}'", LT(0).SourceText(SourceFile.Text));
					ScanToEndOfStmt();
				}
			} while (false);
			// line 1201
			return r;
		}
		LNode IdStmt(int startIndex, VList<LNode> attrs, bool hasWordAttrs)
		{
			TokenType la1;
			LNode result = default(LNode);
			bool addAttrs = false;
			string showWordAttrErrorFor = null;
			// Line 1210: ( Constructor / BlockCallStmt / LabelStmt / &(DataType @`.`(TT, noMacro(@this))) DataType => MethodOrPropertyOrVar / ExprStatement )
			do {
				switch (LA0) {
				case TT.ContextualKeyword:
				case TT.Id:
					{
						if (Try_IdStmt_Test0(0)) {
							if (_spaceName == LT(0).Value) {
								switch (LA(1)) {
								case TT.LParen:
									{
										if (Try_BlockCallStmt_Test0(1)) {
											if (Try_Constructor_Test0(1) || Try_Constructor_Test2(1))
												goto matchConstructor;
											else
												goto matchBlockCallStmt;
										} else if (Try_Constructor_Test0(1) || Try_Constructor_Test2(1))
											goto matchConstructor;
										else
											result = MethodOrPropertyOrVar(startIndex, attrs);
									}
									break;
								case TT.Forward:
								case TT.LBrace:
									{
										if (Try_BlockCallStmt_Test0(1))
											goto matchBlockCallStmt;
										else
											result = MethodOrPropertyOrVar(startIndex, attrs);
									}
									break;
								case TT.Colon:
									goto matchLabelStmt;
								default:
									result = MethodOrPropertyOrVar(startIndex, attrs);
									break;
								}
							} else {
								switch (LA(1)) {
								case TT.LParen:
									{
										if (Try_Constructor_Test2(1))
											goto matchConstructor;
										else if (Try_BlockCallStmt_Test0(1))
											goto matchBlockCallStmt;
										else
											result = MethodOrPropertyOrVar(startIndex, attrs);
									}
									break;
								case TT.Forward:
								case TT.LBrace:
									{
										if (Try_BlockCallStmt_Test0(1))
											goto matchBlockCallStmt;
										else
											result = MethodOrPropertyOrVar(startIndex, attrs);
									}
									break;
								case TT.Colon:
									goto matchLabelStmt;
								default:
									result = MethodOrPropertyOrVar(startIndex, attrs);
									break;
								}
							}
						} else if (_spaceName == LT(0).Value) {
							la1 = LA(1);
							if (la1 == TT.LParen) {
								if (Try_BlockCallStmt_Test0(1)) {
									if (Try_Constructor_Test0(1) || Try_Constructor_Test2(1))
										goto matchConstructor;
									else
										goto matchBlockCallStmt;
								} else if (Try_Constructor_Test0(1) || Try_Constructor_Test2(1))
									goto matchConstructor;
								else
									goto matchExprStatement;
							} else if (la1 == TT.LBrace) {
								if (Try_BlockCallStmt_Test0(1))
									goto matchBlockCallStmt;
								else
									goto matchExprStatement;
							} else if (la1 == TT.Forward)
								goto matchBlockCallStmt;
							else if (la1 == TT.Colon)
								goto matchLabelStmt;
							else
								goto matchExprStatement;
						} else {
							la1 = LA(1);
							if (la1 == TT.LParen) {
								if (Try_Constructor_Test2(1))
									goto matchConstructor;
								else if (Try_BlockCallStmt_Test0(1))
									goto matchBlockCallStmt;
								else
									goto matchExprStatement;
							} else if (la1 == TT.LBrace) {
								if (Try_BlockCallStmt_Test0(1))
									goto matchBlockCallStmt;
								else
									goto matchExprStatement;
							} else if (la1 == TT.Forward)
								goto matchBlockCallStmt;
							else if (la1 == TT.Colon)
								goto matchLabelStmt;
							else
								goto matchExprStatement;
						}
					}
					break;
				case TT.@this:
					{
						if (_spaceName != S.Fn || LA(0 + 3) == TT.LBrace) {
							la1 = LA(1);
							if (la1 == TT.LParen) {
								if (Try_Constructor_Test1(1) || Try_Constructor_Test2(1))
									goto matchConstructor;
								else
									goto matchExprStatement;
							} else
								goto matchExprStatement;
						} else {
							la1 = LA(1);
							if (la1 == TT.LParen) {
								if (Try_Constructor_Test2(1))
									goto matchConstructor;
								else
									goto matchExprStatement;
							} else
								goto matchExprStatement;
						}
					}
				case TT.@default:
					{
						la1 = LA(1);
						if (la1 == TT.Colon)
							goto matchLabelStmt;
						else
							goto matchExprStatement;
					}
				case TT.@operator:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						if (Try_IdStmt_Test0(0))
							result = MethodOrPropertyOrVar(startIndex, attrs);
						else
							goto matchExprStatement;
					}
					break;
				default:
					goto matchExprStatement;
				}
				break;
			matchConstructor:
				{
					result = Constructor(startIndex, attrs);
					// line 1211
					showWordAttrErrorFor = "old-style constructor";
				}
				break;
			matchBlockCallStmt:
				{
					result = BlockCallStmt(startIndex);
					// line 1213
					showWordAttrErrorFor = "block-call statement";
					addAttrs = true;
				}
				break;
			matchLabelStmt:
				{
					result = LabelStmt(startIndex);
					// line 1215
					addAttrs = true;
				}
				break;
			matchExprStatement:
				{
					result = ExprStatement();
					// line 1219
					showWordAttrErrorFor = "expression";
					addAttrs = true;
				}
			} while (false);
			// line 1222
			if (addAttrs) {
				result = result.PlusAttrs(attrs);
			}
			if (hasWordAttrs && showWordAttrErrorFor != null) {
				NonKeywordAttrError(attrs, showWordAttrErrorFor);
			}
			return result;
		}
		static readonly HashSet<int> OtherStmt_set0 = NewSet((int) EOF, (int) TT.@as, (int) TT.@catch, (int) TT.@else, (int) TT.@finally, (int) TT.@in, (int) TT.@is, (int) TT.@using, (int) TT.@while, (int) TT.Add, (int) TT.And, (int) TT.AndBits, (int) TT.BQString, (int) TT.ColonColon, (int) TT.CompoundSet, (int) TT.DivMod, (int) TT.Dot, (int) TT.DotDot, (int) TT.EqNeq, (int) TT.GT, (int) TT.IncDec, (int) TT.LambdaArrow, (int) TT.LBrace, (int) TT.LBrack, (int) TT.LEGE, (int) TT.LParen, (int) TT.LT, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.NullCoalesce, (int) TT.NullDot, (int) TT.OrBits, (int) TT.OrXor, (int) TT.Power, (int) TT.PtrArrow, (int) TT.QuestionMark, (int) TT.QuickBind, (int) TT.Semicolon, (int) TT.Set, (int) TT.Sub, (int) TT.XorBits);
		LNode OtherStmt(int startIndex, VList<LNode> attrs, bool hasWordAttrs)
		{
			TokenType la1;
			Token lit_semi = default(Token);
			LNode result = default(LNode);
			Token ths = default(Token);
			bool addAttrs = false;
			string showWordAttrErrorFor = null;
			// Line 1: (  / IfStmt / BracedBlock / &(TT.NotBits (@`.`(TT, noMacro(@this))|TT.ContextualKeyword|TT.Id) TT.LParen TT.RParen TT.LBrace TT.RBrace) Destructor / TT.Semicolon / LabelStmt / default ExprStatement / AssemblyOrModuleAttribute / OperatorCastMethod / @`.`(TT, noMacro(@this)) &(DataType ComplexNameDecl) MethodOrPropertyOrVar )
			do {
				switch (LA0) {
				case EOF:
					{
					}
					break;
				case TT.@if:
					{
						result = IfStmt(startIndex);
						// line 1240
						showWordAttrErrorFor = "if statement";
						addAttrs = true;
					}
					break;
				case TT.LBrace:
					{
						result = BracedBlock(null, null, startIndex);
						// line 1242
						showWordAttrErrorFor = "braced-block statement";
						addAttrs = true;
					}
					break;
				case TT.NotBits:
					{
						if (Try_OtherStmt_Test0(0)) {
							la1 = LA(1);
							if (la1 == TT.@this || la1 == TT.ContextualKeyword || la1 == TT.Id) {
								result = Destructor(startIndex, attrs);
								// line 1245
								showWordAttrErrorFor = "destructor";
							} else
								goto matchExprStatement;
						} else
							goto matchExprStatement;
					}
					break;
				case TT.Semicolon:
					{
						lit_semi = MatchAny();
						// line 1246
						result = F.Id(S.Missing, startIndex, lit_semi.EndIndex);
						showWordAttrErrorFor = "empty statement";
						addAttrs = true;
					}
					break;
				case TT.ContextualKeyword:
					{
						if (!(_insideLinqExpr && LinqKeywords.Contains(LT(0).Value))) {
							la1 = LA(1);
							if (la1 == TT.Colon)
								goto matchLabelStmt;
							else
								goto matchExprStatement;
						} else
							goto matchLabelStmt;
					}
				case TT.Id:
					{
						la1 = LA(1);
						if (la1 == TT.Colon)
							goto matchLabelStmt;
						else
							goto matchExprStatement;
					}
				case TT.@default:
					{
						la1 = LA(1);
						if (la1 == TT.Colon)
							goto matchLabelStmt;
						else
							goto matchExprStatement;
					}
				case TT.Add:
				case TT.AndBits:
				case TT.Dot:
				case TT.DotDot:
				case TT.Forward:
				case TT.IncDec:
				case TT.LParen:
				case TT.Mul:
				case TT.Not:
				case TT.Power:
				case TT.Sub:
				case TT.Substitute:
					goto matchExprStatement;
				case TT.@operator:
					{
						la1 = LA(1);
						switch (la1) {
						case TT.@operator:
						case TT.ContextualKeyword:
						case TT.Id:
						case TT.TypeKeyword:
							{
								result = OperatorCastMethod(startIndex, attrs);
								// line 1254
								attrs.Clear();
							}
							break;
						default:
							if (AnyOperator_set0.Contains((int) la1))
								goto matchExprStatement;
							else
								goto matchExprStatement;
						}
					}
					break;
				case TT.@this:
					{
						la1 = LA(1);
						switch (la1) {
						case TT.@operator:
						case TT.ContextualKeyword:
						case TT.Id:
						case TT.Substitute:
						case TT.TypeKeyword:
							{
								ths = MatchAny();
								Check(Try_OtherStmt_Test1(0), "DataType ComplexNameDecl");
								// line 1257
								attrs.Add(F.Id(ths));
								result = MethodOrPropertyOrVar(startIndex, attrs);
							}
							break;
						default:
							if (OtherStmt_set0.Contains((int) la1))
								goto matchExprStatement;
							else
								goto matchExprStatement;
						}
					}
					break;
				case TT.@base:
				case TT.@checked:
				case TT.@delegate:
				case TT.@is:
				case TT.@new:
				case TT.@sizeof:
				case TT.@typeof:
				case TT.@unchecked:
				case TT.At:
				case TT.Literal:
				case TT.TypeKeyword:
					goto matchExprStatement;
				case TT.LBrack:
					{
						result = AssemblyOrModuleAttribute(startIndex, attrs);
						// line 1253
						showWordAttrErrorFor = "assembly or module attribute";
					}
					break;
				default:
					goto matchExprStatement;
				}
				break;
			matchLabelStmt:
				{
					result = LabelStmt(startIndex);
					// line 1249
					addAttrs = true;
				}
				break;
			matchExprStatement:
				{
					result = ExprStatement();
					// line 1251
					showWordAttrErrorFor = "expression";
					addAttrs = true;
				}
			} while (false);
			// line 1261
			if (addAttrs) {
				result = result.PlusAttrs(attrs);
			}
			if (hasWordAttrs && showWordAttrErrorFor != null) {
				NonKeywordAttrError(attrs, showWordAttrErrorFor);
			}
			return result;
		}
		LNode ExprStatement()
		{
			LNode result = default(LNode);
			result = Expr(ContinueExpr);
			// Line 1272: ((EOF|TT.@catch|TT.@else|TT.@finally|TT.@while) =>  | TT.Semicolon)
			switch (LA0) {
			case EOF:
			case TT.@catch:
			case TT.@else:
			case TT.@finally:
			case TT.@while:
				// line 1273
				result = F.Call(S.Result, result, result.Range.StartIndex, result.Range.EndIndex);
				break;
			case TT.Semicolon:
				Skip();
				break;
			default:
				{
					// line 1275
					result = Error("Syntax error in expression at '{0}'; possibly missing semicolon", LT(0).SourceText(SourceFile.Text));
					ScanToEndOfStmt();
				}
				break;
			}
			return result;
		}
		void ScanToEndOfStmt()
		{
			TokenType la0;
			// Line 1282: greedy(~(EOF|TT.LBrace|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (!(la0 == (TokenType) EOF || la0 == TT.LBrace || la0 == TT.Semicolon))
					Skip();
				else
					break;
			}
			// Line 1283: greedy(TT.Semicolon | TT.LBrace (TT.RBrace)?)?
			la0 = LA0;
			if (la0 == TT.Semicolon)
				Skip();
			else if (la0 == TT.LBrace) {
				Skip();
				// Line 1283: (TT.RBrace)?
				la0 = LA0;
				if (la0 == TT.RBrace)
					Skip();
			}
		}
		LNode SpaceDecl(int startIndex)
		{
			var t = MatchAny();
			// line 1292
			var kind = (Symbol) t.Value;
			var r = RestOfSpaceDecl(startIndex, kind);
			// line 1294
			return r;
		}
		LNode TraitDecl(int startIndex)
		{
			Check(Is(0, _trait), "Is($LI, _trait)");
			var t = Match((int) TT.ContextualKeyword);
			var r = RestOfSpaceDecl(startIndex, S.Trait);
			// line 1300
			return r;
		}
		LNode RestOfSpaceDecl(int startIndex, Symbol kind)
		{
			TokenType la0;
			var name = ComplexNameDecl();
			var bases = BaseListOpt();
			WhereClausesOpt(ref name);
			// Line 1307: (TT.Semicolon | BracedBlock)
			la0 = LA0;
			if (la0 == TT.Semicolon) {
				var end = MatchAny();
				// line 1308
				return F.Call(kind, name, bases, startIndex, end.EndIndex);
			} else {
				var body = BracedBlock(CoreName(name).Name);
				// line 1310
				return F.Call(kind, name, bases, body, startIndex, body.Range.EndIndex);
			}
		}
		LNode AliasDecl(int startIndex)
		{
			LNode result = default(LNode);
			Check(Is(0, _alias), "Is($LI, _alias)");
			var t = Match((int) TT.ContextualKeyword);
			var newName = ComplexNameDecl();
			Match((int) TT.QuickBindSet, (int) TT.Set);
			var oldName = ComplexNameDecl();
			result = RestOfAlias(oldName, startIndex, newName);
			return result;
		}
		LNode UsingDirective(int startIndex, VList<LNode> attrs)
		{
			TokenType la0;
			Token end = default(Token);
			LNode nsName = default(LNode);
			Token static_ = default(Token);
			Token t = default(Token);
			t = Match((int) TT.@using);
			// Line 1325: (&{Is($LI, S.Static)} TT.AttrKeyword ExprStart TT.Semicolon / ExprStart (&{nsName.Calls(S.Assign, 2) && (aliasedType = nsName[1]) != @null} RestOfAlias / TT.Semicolon))
			do {
				la0 = LA0;
				if (la0 == TT.AttrKeyword) {
					if (Is(0, S.Static)) {
						static_ = MatchAny();
						nsName = ExprStart(true);
						end = Match((int) TT.Semicolon);
						// line 1327
						attrs.Add(F.Id(static_));
					} else
						goto matchExprStart;
				} else
					goto matchExprStart;
				break;
			matchExprStart:
				{
					nsName = ExprStart(true);
					// line 1329
					LNode aliasedType = null;
					// Line 1331: (&{nsName.Calls(S.Assign, 2) && (aliasedType = nsName[1]) != @null} RestOfAlias / TT.Semicolon)
					do {
						la0 = LA0;
						if (la0 == TT.Semicolon) {
							if (nsName.Calls(S.Assign, 2) && (aliasedType = nsName[1]) != null)
								goto matchRestOfAlias;
							else
								end = MatchAny();
						} else
							goto matchRestOfAlias;
						break;
					matchRestOfAlias:
						{
							Check(nsName.Calls(S.Assign, 2) && (aliasedType = nsName[1]) != null, "nsName.Calls(S.Assign, 2) && (aliasedType = nsName[1]) != @null");
							// line 1332
							nsName = nsName[0];
							var r = RestOfAlias(aliasedType, startIndex, nsName);
							// line 1334
							return r.WithAttrs(attrs).PlusAttr(_filePrivate);
						}
					} while (false);
				}
			} while (false);
			// line 1338
			return F.Call(S.Import, nsName, t.StartIndex, end.EndIndex).WithAttrs(attrs);
		}
		LNode RestOfAlias(LNode oldName, int startIndex, LNode newName)
		{
			TokenType la0;
			var bases = BaseListOpt();
			WhereClausesOpt(ref newName);
			// line 1344
			var name = F.Call(S.Assign, newName, oldName, newName.Range.StartIndex, oldName.Range.EndIndex);
			// Line 1345: (TT.Semicolon | BracedBlock)
			la0 = LA0;
			if (la0 == TT.Semicolon) {
				var end = MatchAny();
				// line 1346
				return F.Call(S.Alias, name, bases, startIndex, end.EndIndex);
			} else {
				var body = BracedBlock(CoreName(newName).Name);
				// line 1348
				return F.Call(S.Alias, name, bases, body, startIndex, body.Range.EndIndex);
			}
		}
		LNode EnumDecl(int startIndex)
		{
			TokenType la0;
			var t = MatchAny();
			var name = ComplexNameDecl();
			var bases = BaseListOpt();
			// Line 1356: (TT.Semicolon | TT.LBrace TT.RBrace)
			la0 = LA0;
			if (la0 == TT.Semicolon) {
				var end = MatchAny();
				// line 1357
				return F.Call(S.Enum, name, bases, startIndex, end.EndIndex);
			} else {
				var lb = Match((int) TT.LBrace);
				var rb = Match((int) TT.RBrace);
				// line 1360
				var list = ExprListInside(lb, true);
				var body = F.Braces(list, lb.StartIndex, rb.EndIndex);
				return F.Call(S.Enum, name, bases, body, startIndex, body.Range.EndIndex);
			}
		}
		LNode BaseListOpt()
		{
			TokenType la0;
			// Line 1368: (TT.Colon DataType (TT.Comma DataType)* | )
			la0 = LA0;
			if (la0 == TT.Colon) {
				// line 1368
				var bases = new VList<LNode>();
				Skip();
				bases.Add(DataType());
				// Line 1370: (TT.Comma DataType)*
				for (;;) {
					la0 = LA0;
					if (la0 == TT.Comma) {
						Skip();
						bases.Add(DataType());
					} else
						break;
				}
				// line 1371
				return F.List(bases);
			} else
				// line 1372
				return F.List();
		}
		void WhereClausesOpt(ref LNode name)
		{
			TokenType la0;
			// line 1394
			var list = new BMultiMap<Symbol,LNode>();
			// Line 1395: (WhereClause)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.ContextualKeyword)
					list.Add(WhereClause());
				else
					break;
			}
			// line 1396
			if ((list.Count != 0)) {
				if ((!name.CallsMin(S.Of, 2))) {
					Error("'{0}' is not generic and cannot use 'where' clauses.", name.ToString());
				} else {
					var tparams = name.Args.ToWList();
					for (int i = 1; i < tparams.Count; i++) {
						var wheres = list[TParamSymbol(tparams[i])];
						tparams[i] = tparams[i].PlusAttrs(wheres);
						wheres.Clear();
					}
					name = name.WithArgs(tparams.ToVList());
					if ((list.Count > 0)) {
						Error(list[0].Value, "There is no type parameter named '{0}'", list[0].Key);
					}
				}
			}
		}
		KeyValuePair<Symbol,LNode> WhereClause()
		{
			TokenType la0;
			Check(Is(0, _where), "Is($LI, _where)");
			var where = MatchAny();
			var T = Match((int) TT.ContextualKeyword, (int) TT.Id);
			Match((int) TT.Colon);
			// line 1426
			var constraints = VList<LNode>.Empty;
			constraints.Add(WhereConstraint());
			// Line 1428: (TT.Comma WhereConstraint)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.Comma) {
					Skip();
					constraints.Add(WhereConstraint());
				} else
					break;
			}
			// line 1429
			return new KeyValuePair<Symbol,LNode>((Symbol) T.Value, F.Call(S.Where, constraints, where.StartIndex, constraints.Last.Range.EndIndex));
		}
		LNode WhereConstraint()
		{
			TokenType la0;
			// Line 1433: ( (TT.@class|TT.@struct) | TT.@new &{LT($LI).Count == 0} TT.LParen TT.RParen | DataType )
			la0 = LA0;
			if (la0 == TT.@class || la0 == TT.@struct) {
				var t = MatchAny();
				// line 1433
				return F.Id(t);
			} else if (la0 == TT.@new) {
				var n = MatchAny();
				Check(LT(0).Count == 0, "LT($LI).Count == 0");
				var lp = Match((int) TT.LParen);
				var rp = Match((int) TT.RParen);
				// line 1435
				return F.Call(S.New, n.StartIndex, rp.EndIndex);
			} else {
				var t = DataType();
				// line 1436
				return t;
			}
		}
		Token AsmOrModLabel()
		{
			Check(LT(0).Value == _assembly || LT(0).Value == _module, "LT($LI).Value == _assembly || LT($LI).Value == _module");
			var t = Match((int) TT.ContextualKeyword);
			Match((int) TT.Colon);
			// line 1451
			return t;
		}
		bool Try_Scan_AsmOrModLabel(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Scan_AsmOrModLabel();
		}
		bool Scan_AsmOrModLabel()
		{
			if (!(LT(0).Value == _assembly || LT(0).Value == _module))
				return false;
			if (!TryMatch((int) TT.ContextualKeyword))
				return false;
			if (!TryMatch((int) TT.Colon))
				return false;
			return true;
		}
		LNode AssemblyOrModuleAttribute(int startIndex, VList<LNode> attrs)
		{
			Check(Down(0) && Up(Try_Scan_AsmOrModLabel(0)), "Down($LI) && Up(Try_Scan_AsmOrModLabel(0))");
			var lb = MatchAny();
			var rb = Match((int) TT.RBrack);
			// line 1457
			Down(lb);
			var kind = AsmOrModLabel();
			// line 1459
			var list = new WList<LNode>();
			ExprList(list);
			// line 1462
			Up();
			var r = F.Call(kind.Value == _module ? S.Module : S.Assembly, list.ToVList(), startIndex, rb.EndIndex);
			return r.WithAttrs(attrs);
		}
		LNode MethodOrPropertyOrVar(int startIndex, VList<LNode> attrs)
		{
			TokenType la0;
			LNode name = default(LNode);
			LNode result = default(LNode);
			// line 1474
			bool hasThis;
			var type = DataType();
			name = ComplexNameDecl(true, out hasThis);
			// Line 1478: ( &{!hasThis} VarInitializerOpt (TT.Comma ComplexNameDecl VarInitializerOpt)* TT.Semicolon / &{!hasThis} MethodArgListAndBody | RestOfPropertyDefinition )
			switch (LA0) {
			case TT.Comma:
			case TT.QuickBindSet:
			case TT.Semicolon:
			case TT.Set:
				{
					Check(!hasThis, "!hasThis");
					MaybeRecognizeVarAsKeyword(ref type);
					var parts = LNode.List(type);
					var isArray = IsArrayType(type);
					parts.Add(VarInitializerOpt(name, isArray));
					// Line 1483: (TT.Comma ComplexNameDecl VarInitializerOpt)*
					for (;;) {
						la0 = LA0;
						if (la0 == TT.Comma) {
							Skip();
							name = ComplexNameDecl();
							parts.Add(VarInitializerOpt(name, isArray));
						} else
							break;
					}
					var end = Match((int) TT.Semicolon);
					// line 1486
					result = F.Call(S.Var, parts, type.Range.StartIndex, end.EndIndex);
				}
				break;
			case TT.LParen:
				{
					Check(!hasThis, "!hasThis");
					result = MethodArgListAndBody(startIndex, attrs, S.Fn, type, name);
					// line 1490
					return result;
				}
				break;
			case TT.At:
			case TT.ContextualKeyword:
			case TT.Forward:
			case TT.LambdaArrow:
			case TT.LBrace:
			case TT.LBrack:
				result = RestOfPropertyDefinition(type, name, false);
				break;
			default:
				{
					// line 1493
					Error("Syntax error in what appears to be a method, property, or variable declaration");
					ScanToEndOfStmt();
					// line 1495
					result = F.Call(S.Var, type, name, type.Range.StartIndex, name.Range.EndIndex);
				}
				break;
			}
			result = result.PlusAttrs(attrs);
			return result;
		}
		LNode VarInitializerOpt(LNode name, bool isArray)
		{
			TokenType la0;
			LNode expr = default(LNode);
			// Line 1501: (VarInitializer)?
			la0 = LA0;
			if (la0 == TT.QuickBindSet || la0 == TT.Set) {
				expr = VarInitializer(isArray);
				// line 1502
				return F.Call(S.Assign, name, expr, name.Range.StartIndex, expr.Range.EndIndex);
			}
			// line 1503
			return name;
		}
		LNode VarInitializer(bool isArray)
		{
			TokenType la0;
			LNode result = default(LNode);
			Skip();
			// Line 1510: (&{isArray} &{Down($LI) && Up(HasNoSemicolons())} TT.LBrace TT.RBrace / ExprStart)
			la0 = LA0;
			if (la0 == TT.LBrace) {
				if (Down(0) && Up(HasNoSemicolons())) {
					if (isArray) {
						var lb = MatchAny();
						var rb = Match((int) TT.RBrace);
						// line 1514
						var initializers = InitializerListInside(lb).ToVList();
						result = F.Call(S.ArrayInit, initializers, lb.StartIndex, rb.EndIndex).SetBaseStyle(NodeStyle.OldStyle);
					} else
						result = ExprStart(false);
				} else
					result = ExprStart(false);
			} else
				result = ExprStart(false);
			return result;
		}
		LNode RestOfPropertyDefinition(LNode type, LNode name, bool isExpression)
		{
			TokenType la0;
			Token lb = default(Token);
			Token rb = default(Token);
			LNode result = default(LNode);
			WhereClausesOpt(ref name);
			// line 1524
			LNode args = F.Missing;
			// Line 1525: (TT.LBrack TT.RBrack)?
			la0 = LA0;
			if (la0 == TT.LBrack) {
				lb = MatchAny();
				rb = Match((int) TT.RBrack);
				// line 1525
				args = ArgList(lb, rb);
			}
			// line 1526
			LNode initializer;
			var body = MethodBodyOrForward(true, out initializer, isExpression);
			result = (initializer != null ? F.Property(type, name, args, body, initializer, type.Range.StartIndex, initializer.Range.EndIndex) : F.Property(type, name, args, body, null, type.Range.StartIndex, body.Range.EndIndex));
			return result;
		}
		LNode OperatorCastMethod(int startIndex, VList<LNode> attrs)
		{
			// line 1535
			LNode r;
			var op = MatchAny();
			var type = DataType();
			// line 1537
			var name = F.Attr(_triviaUseOperatorKeyword, F.Id(S.Cast, op.StartIndex, op.EndIndex));
			r = MethodArgListAndBody(startIndex, attrs, S.Fn, type, name);
			// line 1539
			return r;
		}
		LNode MethodArgListAndBody(int startIndex, VList<LNode> attrs, Symbol kind, LNode type, LNode name)
		{
			TokenType la0;
			var lp = Match((int) TT.LParen);
			var rp = Match((int) TT.RParen);
			WhereClausesOpt(ref name);
			// line 1545
			LNode r, baseCall = null;
			// Line 1546: (TT.Colon (@`.`(TT, noMacro(@base))|@`.`(TT, noMacro(@this))) TT.LParen TT.RParen)?
			la0 = LA0;
			if (la0 == TT.Colon) {
				Skip();
				var target = Match((int) TT.@base, (int) TT.@this);
				var baselp = Match((int) TT.LParen);
				var baserp = Match((int) TT.RParen);
				// line 1548
				baseCall = F.Call((Symbol) target.Value, ExprListInside(baselp), target.StartIndex, baserp.EndIndex);
				if ((kind != S.Constructor)) {
					Error(baseCall, "This is not a constructor declaration, so there should be no ':' clause.");
				}
			}
			// line 1555
			for (int i = 0; i < attrs.Count; i++) {
				var attr = attrs[i];
				if (IsNamedArg(attr) && attr.Args[0].IsIdNamed(S.Return)) {
					type = type.PlusAttr(attr.Args[1]);
					attrs.RemoveAt(i);
					i--;
				}
			}
			// Line 1564: (default TT.Semicolon | MethodBodyOrForward)
			do {
				switch (LA0) {
				case TT.Semicolon:
					goto match1;
				case TT.At:
				case TT.Forward:
				case TT.LambdaArrow:
				case TT.LBrace:
					{
						var body = MethodBodyOrForward();
						// line 1576
						if (kind == S.Delegate) {
							Error("A 'delegate' is not expected to have a method body.");
						}
						if (baseCall != null) {
							body = body.WithArgs(body.Args.Insert(0, baseCall)).WithRange(baseCall.Range.StartIndex, body.Range.EndIndex);
						}
						var parts = new VList<LNode> { 
							type, name, ArgList(lp, rp), body
						};
						r = F.Call(kind, parts, startIndex, body.Range.EndIndex);
					}
					break;
				default:
					goto match1;
				}
				break;
			match1:
				{
					var end = Match((int) TT.Semicolon);
					// line 1566
					if (kind == S.Constructor && baseCall != null) {
						Error(baseCall, "A method body is required.");
						var parts = new VList<LNode> { 
							type, name, ArgList(lp, rp), LNode.Call(S.Braces, new VList<LNode>(baseCall), baseCall.Range)
						};
						return F.Call(kind, parts, startIndex, baseCall.Range.EndIndex);
					}
					r = F.Call(kind, type, name, ArgList(lp, rp), startIndex, end.EndIndex);
				}
			} while (false);
			// line 1584
			return r.PlusAttrs(attrs);
		}
		LNode MethodBodyOrForward(bool isProperty, out LNode propInitializer, bool isExpression = false)
		{
			TokenType la0;
			// line 1589
			propInitializer = null;
			// Line 1590: ( TT.Forward ExprStart StmtSemicolon | TT.LambdaArrow ExprStart StmtSemicolon | TokenLiteral StmtSemicolon | BracedBlock greedy(&{isProperty} TT.Set ExprStart StmtSemicolon)? )
			la0 = LA0;
			if (la0 == TT.Forward) {
				var op = MatchAny();
				var e = ExprStart(true);
				StmtSemicolon(isExpression);
				// line 1590
				return F.Call(S.Forward, e, op.StartIndex, e.Range.EndIndex);
			} else if (la0 == TT.LambdaArrow) {
				var op = MatchAny();
				var e = ExprStart(false);
				StmtSemicolon(isExpression);
				// line 1591
				return e;
			} else if (la0 == TT.At) {
				var e = TokenLiteral();
				StmtSemicolon(isExpression);
				// line 1592
				return e;
			} else {
				var body = BracedBlock(S.Fn);
				// Line 1596: greedy(&{isProperty} TT.Set ExprStart StmtSemicolon)?
				la0 = LA0;
				if (la0 == TT.Set) {
					Check(isProperty, "isProperty");
					Skip();
					propInitializer = ExprStart(false);
					StmtSemicolon(isExpression);
				}
				// line 1599
				return body;
			}
		}
		void StmtSemicolon(bool isExpression)
		{
			TokenType la0;
			// Line 1604: (&{isExpression} / TT.Semicolon)
			la0 = LA0;
			if (la0 == TT.Semicolon) {
				if (isExpression)
					Check(isExpression, "isExpression");
				else
					Skip();
			} else
				Check(isExpression, "isExpression");
		}
		void NoSemicolons()
		{
			TokenType la0;
			// Line 1625: (~(EOF|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (!(la0 == (TokenType) EOF || la0 == TT.Semicolon))
					Skip();
				else
					break;
			}
			Match((int) EOF);
		}
		bool Try_HasNoSemicolons(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return HasNoSemicolons();
		}
		bool HasNoSemicolons()
		{
			TokenType la0;
			// Line 1625: (~(EOF|TT.Semicolon))*
			for (;;) {
				la0 = LA0;
				if (!(la0 == (TokenType) EOF || la0 == TT.Semicolon))
					{if (!TryMatchExcept((int) TT.Semicolon))
						return false;}
				else
					break;
			}
			if (!TryMatch((int) EOF))
				return false;
			return true;
		}
		LNode Constructor(int startIndex, VList<LNode> attrs)
		{
			TokenType la0;
			// line 1632
			LNode r;
			Token n;
			// Line 1633: ( &{_spaceName == LT($LI).Value} (TT.ContextualKeyword|TT.Id) &(TT.LParen TT.RParen (TT.LBrace|TT.Semicolon)) / &{_spaceName != S.Fn || LA($LI + 3) == TT.LBrace} @`.`(TT, noMacro(@this)) &(TT.LParen TT.RParen (TT.LBrace|TT.Semicolon)) / (@`.`(TT, noMacro(@this))|TT.ContextualKeyword|TT.Id) &(TT.LParen TT.RParen TT.Colon) )
			do {
				la0 = LA0;
				if (la0 == TT.ContextualKeyword || la0 == TT.Id) {
					if (_spaceName == LT(0).Value) {
						if (Try_Constructor_Test0(1))
							n = MatchAny();
						else
							goto match3;
					} else
						goto match3;
				} else {
					if (_spaceName != S.Fn || LA(0 + 3) == TT.LBrace) {
						if (Try_Constructor_Test1(1))
							n = Match((int) TT.@this);
						else
							goto match3;
					} else
						goto match3;
				}
				break;
			match3:
				{
					n = Match((int) TT.@this, (int) TT.ContextualKeyword, (int) TT.Id);
					Check(Try_Constructor_Test2(0), "TT.LParen TT.RParen TT.Colon");
				}
			} while (false);
			// line 1642
			LNode name = F.Id((Symbol) n.Value, n.StartIndex, n.EndIndex);
			r = MethodArgListAndBody(startIndex, attrs, S.Constructor, F.Missing, name);
			// line 1644
			return r;
		}
		LNode Destructor(int startIndex, VList<LNode> attrs)
		{
			LNode result = default(LNode);
			var tilde = MatchAny();
			var n = MatchAny();
			// line 1650
			var name = (Symbol) n.Value;
			if (name != _spaceName) {
				Error("Unexpected destructor '{0}'", name);
			}
			LNode name2 = F.Call(S.NotBits, F.Id(name, n.StartIndex, n.EndIndex), tilde.StartIndex, n.EndIndex);
			result = MethodArgListAndBody(startIndex, attrs, S.Fn, F.Missing, name2);
			return result;
		}
		LNode DelegateDecl(int startIndex, VList<LNode> attrs)
		{
			Skip();
			var type = DataType();
			var name = ComplexNameDecl();
			var r = MethodArgListAndBody(startIndex, attrs, S.Delegate, type, name);
			// line 1666
			return r.WithAttrs(attrs);
		}
		LNode EventDecl(int startIndex)
		{
			TokenType la0;
			// line 1670
			LNode r;
			Skip();
			var type = DataType();
			var name = ComplexNameDecl();
			// Line 1672: (default (TT.Comma ComplexNameDecl)* TT.Semicolon | BracedBlock)
			do {
				la0 = LA0;
				if (la0 == TT.Comma || la0 == TT.Semicolon)
					goto match1;
				else if (la0 == TT.LBrace) {
					var body = BracedBlock(S.Fn);
					// line 1678
					r = F.Call(S.Event, type, name, body, startIndex, body.Range.EndIndex);
				} else
					goto match1;
				break;
			match1:
				{
					// line 1673
					var parts = new VList<LNode>(type, name);
					// Line 1674: (TT.Comma ComplexNameDecl)*
					for (;;) {
						la0 = LA0;
						if (la0 == TT.Comma) {
							Skip();
							parts.Add(ComplexNameDecl());
						} else
							break;
					}
					var end = Match((int) TT.Semicolon);
					// line 1676
					r = F.Call(S.Event, parts, startIndex, end.EndIndex);
				}
			} while (false);
			// line 1680
			return r;
		}
		LNode LabelStmt(int startIndex)
		{
			var id = Match((int) TT.@default, (int) TT.ContextualKeyword, (int) TT.Id);
			var end = Match((int) TT.Colon);
			// line 1691
			return F.Call(S.Label, F.Id(id), startIndex, end.EndIndex);
		}
		LNode CaseStmt(int startIndex)
		{
			TokenType la0;
			// line 1695
			var cases = VList<LNode>.Empty;
			var kw = Match((int) TT.@case);
			cases.Add(ExprStart2(true));
			// Line 1697: (TT.Comma ExprStart2)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.Comma) {
					Skip();
					cases.Add(ExprStart2(true));
				} else
					break;
			}
			var end = Match((int) TT.Colon);
			// line 1698
			return F.Call(S.Case, cases, startIndex, end.EndIndex);
		}
		LNode BlockCallStmt(int startIndex)
		{
			TokenType la0;
			var id = MatchAny();
			Check(Try_BlockCallStmt_Test0(0), "( TT.LParen TT.RParen (TT.LBrace TT.RBrace | TT.Id) | TT.LBrace TT.RBrace | TT.Forward )");
			var args = new WList<LNode>();
			LNode block;
			// Line 1716: ( TT.LParen TT.RParen (BracedBlock | TT.Id => Stmt) | TT.Forward ExprStart TT.Semicolon | BracedBlock )
			la0 = LA0;
			if (la0 == TT.LParen) {
				var lp = MatchAny();
				var rp = Match((int) TT.RParen);
				// line 1716
				AppendExprsInside(lp, args, false, true);
				// Line 1717: (BracedBlock | TT.Id => Stmt)
				la0 = LA0;
				if (la0 == TT.LBrace)
					block = BracedBlock();
				else {
					block = Stmt();
					// line 1720
					ErrorSink.Write(Severity.Error, block, ColumnOf(block.Range.StartIndex) <= ColumnOf(id.StartIndex) ? "Probable missing semicolon before this statement." : "Probable missing braces around body of '{0}' statement.", id.Value);
				}
			} else if (la0 == TT.Forward) {
				var fwd = MatchAny();
				var e = ExprStart(true);
				Match((int) TT.Semicolon);
				// line 1727
				block = SetOperatorStyle(F.Call(S.Forward, e, fwd.StartIndex, e.Range.EndIndex));
			} else
				block = BracedBlock();
			// line 1731
			args.Add(block);
			var result = F.Call((Symbol) id.Value, args.ToVList(), id.StartIndex, block.Range.EndIndex);
			if (block.Calls(S.Forward, 1)) {
				result = F.Attr(_triviaForwardedProperty, result);
			}
			return result.SetBaseStyle(NodeStyle.Special);
		}
		LNode ReturnBreakContinueThrow(int startIndex)
		{
			var kw = MatchAny();
			var e = ExprOrNull(false);
			var end = Match((int) TT.Semicolon);
			// line 1750
			if (e != null)
				 return F.Call((Symbol) kw.Value, e, startIndex, end.EndIndex);
			else
				 return F.Call((Symbol) kw.Value, startIndex, end.EndIndex);
		}
		LNode GotoStmt(int startIndex)
		{
			Skip();
			var e = ExprOrNull(false);
			var end = Match((int) TT.Semicolon);
			// line 1760
			if (e != null)
				 return F.Call(S.Goto, e, startIndex, end.EndIndex);
			else
				 return F.Call(S.Goto, startIndex, end.EndIndex);
		}
		LNode GotoCaseStmt(int startIndex)
		{
			TokenType la0, la1;
			// line 1766
			LNode e = null;
			Skip();
			Skip();
			// Line 1768: (@`.`(TT, noMacro(@default)) | ExprOpt)
			la0 = LA0;
			if (la0 == TT.@default) {
				la1 = LA(1);
				if (la1 == TT.Semicolon) {
					var @def = MatchAny();
					// line 1769
					e = F.Id(S.Default, @def.StartIndex, @def.EndIndex);
				} else
					e = ExprOpt(false);
			} else
				e = ExprOpt(false);
			var end = Match((int) TT.Semicolon);
			// line 1772
			return F.Call(S.GotoCase, e, startIndex, end.EndIndex);
		}
		LNode CheckedOrUncheckedStmt(int startIndex)
		{
			var kw = MatchAny();
			var bb = BracedBlock();
			// line 1780
			return F.Call((Symbol) kw.Value, bb, startIndex, bb.Range.EndIndex);
		}
		LNode DoStmt(int startIndex)
		{
			Skip();
			var block = Stmt();
			Match((int) TT.@while);
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var end = Match((int) TT.Semicolon);
			// line 1788
			var parts = new WList<LNode> { 
				block
			};
			SingleExprInside(p, "while (...)", parts);
			return F.Call(S.DoWhile, parts.ToVList(), startIndex, end.EndIndex);
		}
		LNode WhileStmt(int startIndex)
		{
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1797
			var cond = SingleExprInside(p, "while (...)");
			return F.Call(S.While, cond, block, startIndex, block.Range.EndIndex);
		}
		LNode ForStmt(int startIndex)
		{
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1817
			Down(p);
			var init = ExprOpt(true);
			Match((int) TT.Semicolon);
			var cond = ExprOpt(false);
			Match((int) TT.Semicolon);
			var inc = ExprOpt(false);
			// line 1819
			Up();
			// line 1821
			var parts = new VList<LNode> { 
				init, cond, inc, block
			};
			return F.Call(S.For, parts, startIndex, block.Range.EndIndex);
		}
		LNode ForEachStmt(int startIndex)
		{
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1829
			Down(p);
			var @var = VarIn();
			var list = ExprStart(false);
			// line 1832
			Up();
			// line 1833
			return F.Call(S.ForEach, @var, list, block, startIndex, block.Range.EndIndex);
		}
		static readonly HashSet<int> VarIn_set0 = NewSet((int) TT.@base, (int) TT.@default, (int) TT.@this, (int) TT.@checked, (int) TT.@delegate, (int) TT.@in, (int) TT.@is, (int) TT.@new, (int) TT.@operator, (int) TT.@sizeof, (int) TT.@typeof, (int) TT.@unchecked, (int) TT.Add, (int) TT.And, (int) TT.AndBits, (int) TT.At, (int) TT.Backslash, (int) TT.BQString, (int) TT.Colon, (int) TT.ColonColon, (int) TT.CompoundSet, (int) TT.ContextualKeyword, (int) TT.DivMod, (int) TT.Dot, (int) TT.DotDot, (int) TT.EqNeq, (int) TT.Forward, (int) TT.GT, (int) TT.Id, (int) TT.IncDec, (int) TT.LambdaArrow, (int) TT.LBrace, (int) TT.LEGE, (int) TT.Literal, (int) TT.LParen, (int) TT.LT, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.NullCoalesce, (int) TT.NullDot, (int) TT.OrBits, (int) TT.OrXor, (int) TT.Power, (int) TT.PtrArrow, (int) TT.QuestionMark, (int) TT.QuickBind, (int) TT.QuickBindSet, (int) TT.Set, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword, (int) TT.XorBits);
		LNode VarIn()
		{
			TokenType la1;
			// line 1837
			LNode @var;
			// Line 1838: (&(Atom TT.@in) Atom / VarDeclStart)
			do {
				switch (LA0) {
				case TT.@operator:
				case TT.ContextualKeyword:
				case TT.Id:
				case TT.Substitute:
				case TT.TypeKeyword:
					{
						if (Try_VarIn_Test0(0)) {
							la1 = LA(1);
							if (VarIn_set0.Contains((int) la1))
								goto matchAtom;
							else
								goto matchVarDeclStart;
						} else
							goto matchVarDeclStart;
					}
				default:
					goto matchAtom;
				}
			matchAtom:
				{
					Check(Try_VarIn_Test0(0), "Atom TT.@in");
					@var = Atom();
				}
				break;
			matchVarDeclStart:
				{
					var pair = VarDeclStart();
					// line 1841
					@var = F.Call(S.Var, pair.A, pair.B, pair.A.Range.StartIndex, pair.B.Range.EndIndex);
				}
			} while (false);
			Match((int) TT.@in);
			// line 1844
			return @var;
		}
		LNode IfStmt(int startIndex)
		{
			TokenType la0;
			// line 1850
			LNode @else = null;
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var then = Stmt();
			// Line 1852: greedy(TT.@else Stmt)?
			la0 = LA0;
			if (la0 == TT.@else) {
				Skip();
				@else = Stmt();
			}
			// line 1854
			var cond = SingleExprInside(p, "if (...)");
			if (@else == null)
				 return F.Call(S.If, cond, then, startIndex, then.Range.EndIndex);
			else
				 return F.Call(S.If, cond, then, @else, startIndex, then.Range.EndIndex);
		}
		LNode SwitchStmt(int startIndex)
		{
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1863
			var expr = SingleExprInside(p, "switch (...)");
			return F.Call(S.Switch, expr, block, startIndex, block.Range.EndIndex);
		}
		LNode UsingStmt(int startIndex)
		{
			Skip();
			var p = MatchAny();
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1873
			var expr = SingleExprInside(p, "using (...)");
			return F.Call(S.UsingStmt, expr, block, startIndex, block.Range.EndIndex);
		}
		LNode LockStmt(int startIndex)
		{
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1881
			var expr = SingleExprInside(p, "lock (...)");
			return F.Call(S.Lock, expr, block, startIndex, block.Range.EndIndex);
		}
		LNode FixedStmt(int startIndex)
		{
			Skip();
			var p = Match((int) TT.LParen);
			Match((int) TT.RParen);
			var block = Stmt();
			// line 1889
			var expr = SingleExprInside(p, "lock (...)");
			return F.Call(S.Fixed, expr, block, startIndex, block.Range.EndIndex);
		}
		LNode TryStmt(int startIndex)
		{
			TokenType la0, la1;
			LNode handler = default(LNode);
			Skip();
			var header = Stmt();
			// line 1898
			var parts = new VList<LNode> { 
				header
			};
			LNode varExpr;
			LNode whenExpr;
			// Line 1901: greedy(TT.@catch (TT.LParen TT.RParen / ) (&{Is($LI, _when)} TT.ContextualKeyword TT.LParen TT.RParen / ) Stmt)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.@catch) {
					var kw = MatchAny();
					// Line 1902: (TT.LParen TT.RParen / )
					la0 = LA0;
					if (la0 == TT.LParen) {
						la1 = LA(1);
						if (la1 == TT.RParen) {
							var p = MatchAny();
							Skip();
							// line 1902
							varExpr = SingleExprInside(p, "catch (...)", null, true);
						} else
							// line 1903
							varExpr = MissingHere();
					} else
						// line 1903
						varExpr = MissingHere();
					// Line 1904: (&{Is($LI, _when)} TT.ContextualKeyword TT.LParen TT.RParen / )
					la0 = LA0;
					if (la0 == TT.ContextualKeyword) {
						if (Is(0, _when)) {
							la1 = LA(1);
							if (la1 == TT.LParen) {
								Skip();
								var c = MatchAny();
								Match((int) TT.RParen);
								// line 1905
								whenExpr = SingleExprInside(c, "when (...)");
							} else
								// line 1906
								whenExpr = MissingHere();
						} else
							// line 1906
							whenExpr = MissingHere();
					} else
						// line 1906
						whenExpr = MissingHere();
					handler = Stmt();
					// line 1908
					parts.Add(F.Call(S.Catch, varExpr, whenExpr, handler, kw.StartIndex, handler.Range.EndIndex));
				} else
					break;
			}
			// Line 1911: greedy(TT.@finally Stmt)*
			for (;;) {
				la0 = LA0;
				if (la0 == TT.@finally) {
					var kw = MatchAny();
					handler = Stmt();
					// line 1912
					parts.Add(F.Call(S.Finally, handler, kw.StartIndex, handler.Range.EndIndex));
				} else
					break;
			}
			// line 1915
			var result = F.Call(S.Try, parts, startIndex, parts.Last.Range.EndIndex);
			if (parts.Count == 1) {
				Error(result, "'try': At least one 'catch' or 'finally' clause is required");
			}
			return result;
		}
		LNode ExprOrNull(bool allowUnassignedVarDecl = false)
		{
			TokenType la0;
			// Line 1928: (ExprStart | )
			la0 = LA0;
			if (InParens_ExprOrTuple_set0.Contains((int) la0)) {
				var e = ExprStart(allowUnassignedVarDecl);
				// line 1928
				return e;
			} else
				// line 1929
				return null;
		}
		LNode ExprOpt(bool allowUnassignedVarDecl = false)
		{
			TokenType la0;
			// Line 1932: (ExprStart | )
			la0 = LA0;
			if (InParens_ExprOrTuple_set0.Contains((int) la0)) {
				var e = ExprStart(allowUnassignedVarDecl);
				// line 1932
				return e;
			} else
				// line 1933
				return MissingHere();
		}
		static readonly HashSet<int> ExprList_set0 = NewSet((int) TT.@base, (int) TT.@default, (int) TT.@this, (int) TT.@checked, (int) TT.@delegate, (int) TT.@is, (int) TT.@new, (int) TT.@operator, (int) TT.@sizeof, (int) TT.@typeof, (int) TT.@unchecked, (int) TT.Add, (int) TT.AndBits, (int) TT.At, (int) TT.AttrKeyword, (int) TT.Comma, (int) TT.ContextualKeyword, (int) TT.Dot, (int) TT.DotDot, (int) TT.Forward, (int) TT.Id, (int) TT.IncDec, (int) TT.LBrace, (int) TT.LBrack, (int) TT.Literal, (int) TT.LParen, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.Power, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword);
		void ExprList(WList<LNode> list, bool allowTrailingComma = false, bool allowUnassignedVarDecl = false)
		{
			TokenType la0, la1;
			// Line 1942: nongreedy(ExprOpt (TT.Comma &{allowTrailingComma} EOF / TT.Comma ExprOpt)*)?
			la0 = LA0;
			if (la0 == EOF)
				;
			else {
				list.Add(ExprOpt(allowUnassignedVarDecl));
				// Line 1943: (TT.Comma &{allowTrailingComma} EOF / TT.Comma ExprOpt)*
				for (;;) {
					la0 = LA0;
					if (la0 == TT.Comma) {
						la1 = LA(1);
						if (la1 == EOF) {
							if (allowTrailingComma) {
								Skip();
								Skip();
							} else
								goto match2;
						} else if (ExprList_set0.Contains((int) la1))
							goto match2;
						else
							goto error;
					} else if (la0 == EOF)
						break;
					else
						goto error;
					continue;
				match2:
					{
						Skip();
						list.Add(ExprOpt(allowUnassignedVarDecl));
					}
					continue;
				error:
					{
						// line 1945
						Error("Syntax error in expression list");
						// Line 1945: (~(EOF|TT.Comma))*
						for (;;) {
							la0 = LA0;
							if (!(la0 == (TokenType) EOF || la0 == TT.Comma))
								Skip();
							else
								break;
						}
					}
				}
			}
			Skip();
		}
		void ArgList(WList<LNode> list)
		{
			TokenType la0;
			// Line 1951: nongreedy(ExprOpt (TT.Comma ExprOpt)*)?
			la0 = LA0;
			if (la0 == EOF)
				;
			else {
				list.Add(ExprOpt(true));
				// Line 1952: (TT.Comma ExprOpt)*
				for (;;) {
					la0 = LA0;
					if (la0 == TT.Comma) {
						Skip();
						list.Add(ExprOpt(true));
					} else if (la0 == EOF)
						break;
					else {
						// line 1953
						Error("Syntax error in argument list");
						// Line 1953: (~(EOF|TT.Comma))*
						for (;;) {
							la0 = LA0;
							if (!(la0 == (TokenType) EOF || la0 == TT.Comma))
								Skip();
							else
								break;
						}
					}
				}
			}
			Skip();
		}
		LNode InitializerExpr()
		{
			TokenType la0, la2;
			LNode result = default(LNode);
			// Line 1960: ( TT.LBrace TT.RBrace / TT.LBrack TT.RBrack TT.Set ExprStart / ExprOpt )
			do {
				la0 = LA0;
				if (la0 == TT.LBrace) {
					la2 = LA(2);
					if (la2 == (TokenType) EOF || la2 == TT.Comma) {
						var lb = MatchAny();
						var rb = Match((int) TT.RBrace);
						// line 1962
						var exprs = InitializerListInside(lb).ToVList();
						result = F.Call(S.Braces, exprs, lb.StartIndex, rb.EndIndex).SetBaseStyle(NodeStyle.OldStyle);
					} else
						result = ExprOpt(false);
				} else if (la0 == TT.LBrack) {
					if (!(Down(0) && Up(Try_Scan_AsmOrModLabel(0)))) {
						la2 = LA(2);
						if (la2 == TT.Set)
							goto match2;
						else
							result = ExprOpt(false);
					} else
						goto match2;
				} else
					result = ExprOpt(false);
				break;
			match2:
				{
					var lb = MatchAny();
					var rb = Match((int) TT.RBrack);
					Match((int) TT.Set);
					var e = ExprStart(false);
					// line 1966
					result = F.Call(S.InitializerAssignment, ExprListInside(lb).ToVList().Add(e), lb.StartIndex, e.Range.EndIndex);
				}
			} while (false);
			return result;
		}
		void InitializerList(WList<LNode> list)
		{
			TokenType la0, la1;
			// Line 1973: nongreedy(InitializerExpr (TT.Comma EOF / TT.Comma InitializerExpr)*)?
			la0 = LA0;
			if (la0 == EOF)
				;
			else {
				list.Add(InitializerExpr());
				// Line 1974: (TT.Comma EOF / TT.Comma InitializerExpr)*
				for (;;) {
					la0 = LA0;
					if (la0 == TT.Comma) {
						la1 = LA(1);
						if (la1 == EOF) {
							Skip();
							Skip();
						} else if (ExprList_set0.Contains((int) la1)) {
							Skip();
							list.Add(InitializerExpr());
						} else
							goto error;
					} else if (la0 == EOF)
						break;
					else
						goto error;
					continue;
				error:
					{
						// line 1976
						Error("Syntax error in initializer list");
						// Line 1976: (~(EOF|TT.Comma))*
						for (;;) {
							la0 = LA0;
							if (!(la0 == (TokenType) EOF || la0 == TT.Comma))
								Skip();
							else
								break;
						}
					}
				}
			}
			Skip();
		}
		void StmtList(WList<LNode> list)
		{
			TokenType la0;
			// Line 1981: (~(EOF) => Stmt)*
			for (;;) {
				la0 = LA0;
				if (la0 != (TokenType) EOF)
					list.Add(Stmt());
				else
					break;
			}
			Skip();
		}
		static readonly HashSet<int> TypeSuffixOpt_Test0_set0 = NewSet((int) TT.@new, (int) TT.Add, (int) TT.AndBits, (int) TT.At, (int) TT.Forward, (int) TT.Id, (int) TT.IncDec, (int) TT.LBrace, (int) TT.Literal, (int) TT.LParen, (int) TT.Mul, (int) TT.Not, (int) TT.NotBits, (int) TT.Sub, (int) TT.Substitute, (int) TT.TypeKeyword);
		private bool Try_TypeSuffixOpt_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return TypeSuffixOpt_Test0();
		}
		private bool TypeSuffixOpt_Test0()
		{
			// Line 334: ((TT.@new|TT.Add|TT.AndBits|TT.At|TT.Forward|TT.Id|TT.IncDec|TT.LBrace|TT.Literal|TT.LParen|TT.Mul|TT.Not|TT.NotBits|TT.Sub|TT.Substitute|TT.TypeKeyword) | UnusualId)
			switch (LA0) {
			case TT.@new:
			case TT.Add:
			case TT.AndBits:
			case TT.At:
			case TT.Forward:
			case TT.Id:
			case TT.IncDec:
			case TT.LBrace:
			case TT.Literal:
			case TT.LParen:
			case TT.Mul:
			case TT.Not:
			case TT.NotBits:
			case TT.Sub:
			case TT.Substitute:
			case TT.TypeKeyword:
				if (!TryMatch(TypeSuffixOpt_Test0_set0))
					return false;
				break;
			default:
				if (!Scan_UnusualId())
					return false;
				break;
			}
			return true;
		}
		private bool Try_ExprInParensAuto_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return ExprInParensAuto_Test0();
		}
		private bool ExprInParensAuto_Test0()
		{
			if (!Scan_ExprInParens(true))
				return false;
			if (!TryMatch((int) TT.LambdaArrow, (int) TT.Set))
				return false;
			return true;
		}
		static readonly HashSet<int> FinishPrimaryExpr_Test0_set0 = NewSet((int) TT.ContextualKeyword, (int) TT.Id);
		private bool Try_FinishPrimaryExpr_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return FinishPrimaryExpr_Test0();
		}
		private bool FinishPrimaryExpr_Test0()
		{
			if (!Scan_TParams())
				return false;
			if (!TryMatchExcept(FinishPrimaryExpr_Test0_set0))
				return false;
			return true;
		}
		private bool Try_PrefixExpr_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return PrefixExpr_Test0();
		}
		private bool PrefixExpr_Test0()
		{
			// Line 698: ((TT.Add|TT.BQString|TT.Dot|TT.Sub) | TT.IncDec TT.LParen)
			switch (LA0) {
			case TT.Add:
			case TT.BQString:
			case TT.Dot:
			case TT.Sub:
				if (!TryMatch((int) TT.Add, (int) TT.BQString, (int) TT.Dot, (int) TT.Sub))
					return false;
				break;
			default:
				{
					if (!TryMatch((int) TT.IncDec))
						return false;
					if (!TryMatch((int) TT.LParen))
						return false;
				}
				break;
			}
			return true;
		}
		static readonly HashSet<int> WordAttributes_Test0_set0 = NewSet((int) TT.@break, (int) TT.@continue, (int) TT.@return, (int) TT.@throw, (int) TT.@case, (int) TT.@class, (int) TT.@delegate, (int) TT.@do, (int) TT.@enum, (int) TT.@event, (int) TT.@fixed, (int) TT.@for, (int) TT.@foreach, (int) TT.@goto, (int) TT.@interface, (int) TT.@lock, (int) TT.@namespace, (int) TT.@struct, (int) TT.@switch, (int) TT.@try, (int) TT.@while);
		private bool Try_WordAttributes_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return WordAttributes_Test0();
		}
		private bool WordAttributes_Test0()
		{
			TokenType la0;
			// Line 1048: ( DataType ((TT.AttrKeyword|TT.Id|TT.TypeKeyword) | UnusualId) | @`.`(TT, noMacro(@this)) TT.LParen | (TT.@new|TT.AttrKeyword) | TT.@checked TT.LBrace TT.RBrace | TT.@unchecked TT.LBrace TT.RBrace | @`.`(TT, noMacro(@default)) TT.Colon | TT.@using TT.LParen | (@`.`(TT, noMacro(@break))|@`.`(TT, noMacro(@continue))|@`.`(TT, noMacro(@return))|@`.`(TT, noMacro(@throw))|TT.@case|TT.@class|TT.@delegate|TT.@do|TT.@enum|TT.@event|TT.@fixed|TT.@for|TT.@foreach|TT.@goto|TT.@interface|TT.@lock|TT.@namespace|TT.@struct|TT.@switch|TT.@try|TT.@while) )
			switch (LA0) {
			case TT.@operator:
			case TT.ContextualKeyword:
			case TT.Id:
			case TT.Substitute:
			case TT.TypeKeyword:
				{
					if (!Scan_DataType())
						return false;
					// Line 1048: ((TT.AttrKeyword|TT.Id|TT.TypeKeyword) | UnusualId)
					la0 = LA0;
					if (la0 == TT.AttrKeyword || la0 == TT.Id || la0 == TT.TypeKeyword)
						{if (!TryMatch((int) TT.AttrKeyword, (int) TT.Id, (int) TT.TypeKeyword))
							return false;}
					else if (!Scan_UnusualId())
						return false;
				}
				break;
			case TT.@this:
				{
					if (!TryMatch((int) TT.@this))
						return false;
					if (!TryMatch((int) TT.LParen))
						return false;
				}
				break;
			case TT.@new:
			case TT.AttrKeyword:
				if (!TryMatch((int) TT.@new, (int) TT.AttrKeyword))
					return false;
				break;
			case TT.@checked:
				{
					if (!TryMatch((int) TT.@checked))
						return false;
					if (!TryMatch((int) TT.LBrace))
						return false;
					if (!TryMatch((int) TT.RBrace))
						return false;
				}
				break;
			case TT.@unchecked:
				{
					if (!TryMatch((int) TT.@unchecked))
						return false;
					if (!TryMatch((int) TT.LBrace))
						return false;
					if (!TryMatch((int) TT.RBrace))
						return false;
				}
				break;
			case TT.@default:
				{
					if (!TryMatch((int) TT.@default))
						return false;
					if (!TryMatch((int) TT.Colon))
						return false;
				}
				break;
			case TT.@using:
				{
					if (!TryMatch((int) TT.@using))
						return false;
					if (!TryMatch((int) TT.LParen))
						return false;
				}
				break;
			default:
				if (!TryMatch(WordAttributes_Test0_set0))
					return false;
				break;
			}
			return true;
		}
		private bool Try_IdStmt_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return IdStmt_Test0();
		}
		private bool IdStmt_Test0()
		{
			if (!Scan_DataType())
				return false;
			if (!TryMatch((int) TT.@this))
				return false;
			return true;
		}
		private bool Try_OtherStmt_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return OtherStmt_Test0();
		}
		private bool OtherStmt_Test0()
		{
			if (!TryMatch((int) TT.NotBits))
				return false;
			if (!TryMatch((int) TT.@this, (int) TT.ContextualKeyword, (int) TT.Id))
				return false;
			if (!TryMatch((int) TT.LParen))
				return false;
			if (!TryMatch((int) TT.RParen))
				return false;
			if (!TryMatch((int) TT.LBrace))
				return false;
			if (!TryMatch((int) TT.RBrace))
				return false;
			return true;
		}
		private bool Try_OtherStmt_Test1(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return OtherStmt_Test1();
		}
		private bool OtherStmt_Test1()
		{
			if (!Scan_DataType())
				return false;
			if (!Scan_ComplexNameDecl())
				return false;
			return true;
		}
		private bool Try_Constructor_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Constructor_Test0();
		}
		private bool Constructor_Test0()
		{
			if (!TryMatch((int) TT.LParen))
				return false;
			if (!TryMatch((int) TT.RParen))
				return false;
			if (!TryMatch((int) TT.LBrace, (int) TT.Semicolon))
				return false;
			return true;
		}
		private bool Try_Constructor_Test1(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Constructor_Test1();
		}
		private bool Constructor_Test1()
		{
			if (!TryMatch((int) TT.LParen))
				return false;
			if (!TryMatch((int) TT.RParen))
				return false;
			if (!TryMatch((int) TT.LBrace, (int) TT.Semicolon))
				return false;
			return true;
		}
		private bool Try_Constructor_Test2(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return Constructor_Test2();
		}
		private bool Constructor_Test2()
		{
			if (!TryMatch((int) TT.LParen))
				return false;
			if (!TryMatch((int) TT.RParen))
				return false;
			if (!TryMatch((int) TT.Colon))
				return false;
			return true;
		}
		private bool Try_BlockCallStmt_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return BlockCallStmt_Test0();
		}
		private bool BlockCallStmt_Test0()
		{
			TokenType la0;
			// Line 1713: ( TT.LParen TT.RParen (TT.LBrace TT.RBrace | TT.Id) | TT.LBrace TT.RBrace | TT.Forward )
			la0 = LA0;
			if (la0 == TT.LParen) {
				if (!TryMatch((int) TT.LParen))
					return false;
				if (!TryMatch((int) TT.RParen))
					return false;
				// Line 1713: (TT.LBrace TT.RBrace | TT.Id)
				la0 = LA0;
				if (la0 == TT.LBrace) {
					if (!TryMatch((int) TT.LBrace))
						return false;
					if (!TryMatch((int) TT.RBrace))
						return false;
				} else if (!TryMatch((int) TT.Id))
					return false;
			} else if (la0 == TT.LBrace) {
				if (!TryMatch((int) TT.LBrace))
					return false;
				if (!TryMatch((int) TT.RBrace))
					return false;
			} else if (!TryMatch((int) TT.Forward))
				return false;
			return true;
		}
		private bool Try_VarIn_Test0(int lookaheadAmt)
		{
			using (new SavePosition(this, lookaheadAmt))
				return VarIn_Test0();
		}
		private bool VarIn_Test0()
		{
			if (!Scan_Atom())
				return false;
			if (!TryMatch((int) TT.@in))
				return false;
			return true;
		}
	}
}