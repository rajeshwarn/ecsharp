﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Loyc.Essentials;
using Loyc.Collections;
using Loyc.CompilerCore;
using Loyc.Collections.Impl;
using ecs;

namespace Loyc.LLParserGenerator
{
	using S = CodeSymbols;
	using Loyc.Utilities;

#if false
	TODO: use this table as a test suite for the parser

	General rules:
	- Variables and fields use #var(type, name1, name2(initial_value), name3)
	  Properties use #property(name, type, #{ body; }) instead.
	  The parser treats "var x" as #var(var, x), but #var(#missing, x) is canonical.
	- All spaces have the form #spacekind(name, #(inherited_types), #{body});
	  the third argument is omitted if the body is omitted.
	  e.g. #struct(Point<#T>, #(IPoint), #{ public int X, Y; });
	- Methods, operators and constructors use #def(retType, name, #(args), #{body});
	  the body can be omitted, or replaced with #==>(target) for forwarding.
	  "if" and "where" clauses are attached as #if and #where attributes.
	  e.g. #def(#double, Square, #(double x), #{ return x*x; });

	Standard EC# statements: Declarations       Prefix notation
	-------------------------------------       ---------------
	using System.Collections.Generic;           #import(System.Collections.Generic);
	using System { Linq, Text };                #import (System.Linq, System.Text);
	using Foo = Bar;                            [#fileLocal] #alias(Foo = Bar);
	extern alias Z;                             #extern_alias(Z);
	[assembly:Attr]                             [Attr] #assembly;
	case 123:                                   #case(123);
	default:                                    #label(#default);
	label_name:                                 #label(label_name);
	int x = 0;                                  #var(int, x(0));
	int* a, b = &x, c;                          #var(#*(int), a, b(&x), c);
	public partial class Foo<T> : IFoo {}       [#public, #partial] #class(Foo<T>, #(IFoo), {});
	struct Foo<\T> if default(T) + 0 is legal   [#if(default(T) + 0 is legal)] #struct(Foo<\T>, #missing, {});
	enum Foo : byte { A = 1, B, C, Z = 26 }     #enum(Foo, byte, #(A = 1, B, C, Z = 26));
	trait Foo<\T> : Stream { ... }              #trait(Foo<\T>, #(Stream), {...});
	interface Foo<T> : IEnumerable<T> { ... }   #interface(Foo<T>, #(IEnumerable<T>), {...});
	namespace Foo<T> { ... }                    #namespace(Foo<T>, #missing, {...});
	namespace Foo<T> { ... }                    #namespace(Foo<T>, #missing, {...});
	alias Map<K,V> = Dictionary<K,V>;           #alias(Foo<T> = Bar<T>);
	alias Foo = Bar : IFoo { ... }              #alias(Foo<T> = Bar<T>, #(IFoo), { ... });
	event EventHandler Click;                   #event(EventHandler, Click);
	event EventHandler A, B;                    #event(EventHandler, A, B));
	event EventHandler A { add { } remove { } } #event(EventHandler, A, { add({ }); remove({ }); }));
	delegate void foo<T>(T x) where T:class,X   [#where(T, #class, X)] #delegate(foo<T>, #(T x), void);
	public new partial string foo(int x);       [#public, #partial, #new] #def(#string, foo, #(int x));
	int foo(int x) => x * x;                    #def(int, foo, #(int x), { x * x; });
	int foo(int x) { return x * x; }            #def(int, foo, #(int x), { #return(x * x); });
	def foo(int x) ==> bar;                     [#def] #def(#missing, foo, #(int x), #==>(bar));
	int Foo { get; set; }                       #property(int, Foo, { get; set; })
	IEnumerator IEnumerable.GetEnumerator() { } #def(IEnumerator, IEnumerable.GetEnumerator, #(), { });
	new (int x) : this(x, 0) { y = x; }         #def(#missing, #new, #(int x), { #this(x, 0); y = x; });
	Foo (int x) : base(x) { y = x; }            #def(#missing, Foo,  #(int x), { #base(x); y = x; });
	~Foo () { ... }                             #def(#missing, #~(Foo), #(), { ... });
	static bool operator==(T a, T b) { ... }    [#static] #def(#bool, [#operator] #==, #(T a, T b), { ... });
	static implicit operator A(B b) { ... }     [#static, #implicit] #def(A, [#operator] #cast, #(B b), { ... });
	static explicit operator A<T><\T>(B<T> b);  [#static, #explicit] #def(A<T>, [#operator] #of<#cast, \T>, #(B<T> b));
	bool operator `when`(Cond cond) { ... }     #def(#bool, [#operator] when, #(Cond cond), { ... });

	Standard EC# statements: Executable         Prefix notation
	-----------------------------------         ---------------
	if (c) f();                                 #if(c, f());
	if (c) { f(); }                             #if(c, { f(); });
	if (c) a = 1, b = 2;                        #if(c, #(a = 1, b = 2));
	if (c) f(); else { g(); }                   #if(c, f(), { g() });
	for (int x = 0; x * y < 100; x++) f(x);     #for(#var(int, x(0)), x * y < 100, x++, f(x));
	foreach (var x in list) { ... }             #foreach(#var(var, x), list, { ... }) // not "#in(#var(var, x), list)" because that's unparsable
	while (x > 0) { ... }                       #while(x > 0, { ... })
	switch (c) { case '+', '-': goto default;   #switch(c, { #case('+', '-'); #goto(#default);
	             default: break; }                           #default; #break; }
	checked { ... }                             #checked({ ... })
	unchecked { ... }                           #unchecked({ ... })
	using (d = new Form()) { ... }              #using(d = new Form(), { ... })
	using (IDisposable d = new Form()) { ... }  #using(#var(IDisposable, d(new Form()), { ... })
	


	EC# expressions       Prefix notation            EC# expressions       Prefix notation 
	---------------       ---------------            ---------------       ---------------
	foo (or @foo)         foo                        a + b          
	food.pizza.cheese     #.(food, pizza, cheese)     
	.foo                  #.(foo)                     
	foo<A, B>             #of(foo, A, B)              
	operator ==           #operator(#==)              
	int                   #int                       
	int x = 0             #var(#int, x(0))           
    foo()::x              #:::(foo(), x)               
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      
                                                      

#endif

	/// <summary>A helper class for generating input nodes to the parser generator.</summary>
	/// <remarks>
	/// I was going to use this to bootstrap the EC# parser, but I'm not sure if 
	/// it's necessary now; I should probably just create predicates directly (<see cref="Pred"/>).
	/// </remarks>
	public class PGFactory : GreenFactory
	{
		public PGFactory() : base(EmptySourceFile.Unknown) { }
		public PGFactory(ISourceFile file) : base(file) { }

		public static readonly Symbol _Gate = GSymbol.Get("#=>");
		public static readonly Symbol _Star = GSymbol.Get("#*");
		public static readonly Symbol _Plus = GSymbol.Get("#+");
		public static readonly Symbol _Opt = GSymbol.Get("#?");
		public static readonly Symbol _Nongreedy = GSymbol.Get("nongreedy");
		public static readonly Symbol _Greedy = GSymbol.Get("greedy");
		public static readonly Symbol _rule = GSymbol.Get("rule");
		public static readonly Symbol _token = GSymbol.Get("token");

		public GreenNode Any { get { return Symbol(GSymbol.Get("_")); } } // represents any terminal

		public GreenNode Rule(string name, params GreenAtOffs[] sequence)
		{
			return Def(Symbol(_rule), GSymbol.Get(name), ArgList(), Braces(sequence));
		}
		public GreenNode Seq(params GreenAtOffs[] sequence) { return Call(S.Tuple, sequence); }
		public GreenNode Seq(params char[] sequence) { return Call(S.Tuple, sequence.Select(c => (GreenAtOffs)_(c)).ToArray()); }
		public GreenNode Star(params GreenAtOffs[] sequence) { return Call(_Star, sequence); }
		public GreenNode Plus(params GreenAtOffs[] sequence) { return Call(_Plus, sequence); }
		public GreenNode Opt(params GreenAtOffs[] sequence)  { return Call(_Opt,  sequence); }
		public GreenNode Nongreedy(GreenNode loop) { return Greedy(loop, false); }
		public GreenNode Greedy(GreenNode loop, bool greedy = true)
		{
			Debug.Assert(loop.Name == _Star || loop.Name == _Plus || loop.Name == _Opt);
			return Call(greedy ? _Greedy : _Nongreedy, loop);
		}
		public  GreenNode And(params GreenAtOffs[] sequence)  { return Call(S.AndBits, AutoS(sequence)); }
		public  GreenNode AndNot(params GreenAtOffs[] sequence) { return Call(S.Not, AutoS(sequence)); }
		public  GreenNode AndCode(params GreenAtOffs[] sequence) { return Call(S.AndBits, Code(sequence)); }
		public  GreenNode Code(params GreenAtOffs[] statements) { return Call(S.Braces, statements); }
		private GreenNode AutoS(GreenAtOffs[] sequence)
		{
			return sequence.Length == 1 ? sequence[0].Node : Seq(sequence);
		}
		public GreenNode _(char c)
		{
			return Literal(c);
		}
	}

	/// <summary>Encapsulates LLLPG, the Loyc Parser Generator for LL Parsers.</summary>
	/// <remarks>
	/// Note: the input to LLLPG is usually provided in the form of EC# source code:
	/// <code>
	/// [[GenerateLLParser]]
	/// class Tokenizer
	/// {
	///   token Id ==> #
	///     [ '@' { _verbatim=1; }? NormalIdStart NormalIdCont*
	///     | '@' { _verbatim=1; } SqString
	///     ];
	///   rule IdStart ==> #[ Letter | '_' ];
	///   rule IdCont  ==> #[ IdStart | ('0'..'9') ];
	///   rule Letter  ==> #[ 'a'..'z' | 'A'..'Z' | &{Char.IsLetter(LA0)} . ];
	///   bool _verbatim;
	/// }
	/// </code>
	/// In that case, there is no need to use this class directly.
	/// <para/>
	/// LLParserGenerator's job is to generate a parser (in the form of an EC# Loyc 
	/// tree) for a set of <see cref="Rule"/>s. Each rule represents a sub-parser, 
	/// whose job is to parse a single predicate (Pred object). LLParserGenerator
	/// can generate both lexers (which take characters as input) and parsers (which
	/// take tokens as input), and many low-level details of the parsing process can 
	/// be customized.</li>
	/// <para/>
	/// Parser generation has the following steps:
	/// <ol>
	/// <li><see cref="AddRules"/>(): scan the body of a class or other statement list
	///     and build a set of Rule objects, one per rule() method or property in the 
	///     source code (alternately, you can add Rule objects directly)</li>
	/// <li><see cref="DetermineLocalFollowSets"/>: scan all predicates of all rules 
	///     and notify each predicate of the predicate that follows it by setting 
	///     <see cref="Pred.Next"/>.</li>
	/// <li><see cref="DetermineRuleFollowSets"/>: scan all predicates of all rules 
	///     looking for <see cref="RuleRef"/>s. For each RuleRef found, add the value 
	///     of <see cref="Pred.Next"/> to the follow set of the rule (stored in 
	///     <see cref="Rule.EndOfRule"/>).</li>
	/// <li>The actual parser can now be generated. Generating code for sequences 
	///     like '(' 'a'..'z' ')' is trivial; the biggest difficulty is generating 
	///     prediction code. See below for details about how prediction (and 
	///     matching) works.
	/// </li>
	/// <li></li>
	/// </ol>
	/// Typically, the majority of a parser consists of prediction code. In 
	/// general, the job of prediction code is to figure out, given some input 
	/// sequence, which one of a number of "alternatives" match that sequence. If 
	/// none of the alternatives match the sequence, the input has an error (or 
	/// from a Chomskian perspective, the input cannot be generated by the 
	/// grammar). If more than one alternative matches the input sequence then the 
	/// grammar is ambiguous; in that case the parser will select the first 
	/// alternative that matches. Also, the parser generator will detect the 
	/// ambiguity as it generates the prediction code and issue a warning about it, 
	/// unless the grammar does something to suppress the warning, such as using 
	/// the '/' operator instead of '|', which means "I know it's ambiguous--just
	/// pick the first matching clause".
	/// <para/>
	/// The following kinds of grammar elements require prediction:
	/// <para/>
	/// <ul>
	/// <li><c>a | b</c> (which is equivalent to <c>a / b</c>): prediction chooses between a and b</li>
	/// <li><c>a?</c>: prediction chooses between a and whatever follows a?</li>
	/// <li><c>a*</c>: prediction chooses between a and whatever follows a*</li>
	/// <li><c>(a | b)*: </c>prediction chooses between three alternatives (a, b, and exiting the loop).</li>
	/// <li><c>(a | b)?: </c>prediction chooses between three alternatives (a, b, and nothing)</c>.</li>
	/// <li><c>a+</c>: equivalent to <c>a a*</c></li>
	/// </ul>
	/// Let's look at a simple example of the prediction code generated for a rule 
	/// called "Foo":
	/// <code>
	/// // rule a ==> #[ 'a' | 'A' ];
	/// // rule b ==> #[ 'b' | 'B' ];
	/// // public rule Foo ==> #[ a | b ];
	/// public void Foo()
	/// {
	///   var la0 = LA(0);
	///   if (la0 == 'a' || la0 == 'A')
	///     a();
	///   else
	///     b();
	/// }
	/// </code>
	/// By default, to make prediction more efficient, the last alternative is 
	/// assumed to match if the others don't. So when <c>a</c> doesn't match, <c>b</c>
	/// is called even though it has not been verified to match yet. This behavior
	/// can be changed by putting the <c>[MatchLastByDefault(false)]</c> attribute 
	/// on either a single rule or on the entire parser class (NOT IMPLEMENTED).
	/// Alternatively, you can select the default using the default(...) pseudo-
	/// function, e.g.
	/// <code>
	/// // public rule Foo ==> #[ default(a) | b ];
	/// public void Foo()
	/// {
	///   var la0 = LA(0);
	///   if (la0 == 'b' || la0 == 'B')
	///     b();
	///   else
	///     a();
	/// }
	/// </code>
	/// In general, the prediction and matching phases are physically separated:
	/// First an if-else chain does prediction, and then a second if-else chain
	/// reacts to the choice that was made. However, in simple cases like this one
	/// that only require LL(1) prediction, prediction and matching are merged 
	/// into a single if-else chain. The if-else statements are the prediction 
	/// part of the code, while the calls to a() and b() are the matching part.
	/// <para/>
	/// Here's another example:
	/// <code>
	/// // public rule Foo ==> #[ (a | b? 'c')* ];
	/// public void Foo()
	/// {
	///   for (;;) {
	///     var la0 = LA(0);
	///     if (la0 == 'a' || la0 == 'A')
	///       a();
	///     else if (la0 == 'b' || la0 == 'B' || la0 == 'c') {
	///       do {
	///         la0 = LA(0);
	///         if (la0 == 'b' || la0 == 'B')
	///           b();
	///       } while(false);
	///       Match('c');
	///     }
	///   }
	/// }
	/// </code>
	/// A kleene star (*) always produces a "for(;;)" loop, while an optional item
	/// always produces a "do ... while(false)" pseudo-loop. Here there are two
	/// separate prediction phases: one for the outer loop <c>(a | b? 'c')*</c>,
	/// and one for <c>b?</c>.
	/// <para/>
	/// In this example, the loop appears at the end of the rule. In some such 
	/// cases, the "follow set" of the rule becomes relevant. In order for the 
	/// parser to decide whether to exit the loop or not, it may need to know what 
	/// can follow the loop. For instance, if <c>('a' 'b')*</c> is followed by 
	/// <c>('a'..'z'+)</c>, it is not always possible to tell whether to stay in 
	/// the loop or exit just by looking at the first input character. If LA(0) is 
	/// 'a', it is necessary to look at the second character; only if the second 
	/// character is 'b' is it possible to conclude that 'a' 'b' should be matched.
	/// <para/>
	/// Therefore, before generating a parser one of the steps is to build the 
	/// follow set of each rule, by looking for places where a rule appears inside
	/// other rules. A rule is not aware of its current caller, so it gathers 
	/// information from all call sites. When a rule is marked "public", it is 
	/// considered to be a starting rule, which causes the follow set to include
	/// $ (which means "end of input").
	/// <para/>
	/// The worst-case follow set is <c>.* $</c> (anything), because essentially
	/// the prediction step must look ahead by the maximum number of characters or
	/// tokens (called k, as in "LL(k)") to decide whether to stay in the loop or 
	/// exit, and even after looking k characters ahead it is often impossible to
	/// disambiguate (in particular, the maximum lookahead is required when an arm
	/// contains a loop.)
	/// <para/>
	/// To mitigate this problem, there are separate "k" values for follow sets 
	/// and token follow sets. When generating a lexer, the follow set of any 
	/// token tends to be close to <c>.* $</c>, i.e. tokens can be almost anything.
	/// Specifically, in computer languages the acceptable input is almost the 
	/// entire ASCII character set, plus perhaps unicode characters that can be
	/// used as identifiers. The default <see cref="TokenFollowSetK"/> is 1, which
	/// means "only look ahead one character to decide whether to exit a loop".
	/// You can also set it to 0, which means "don't pay attention to the follow
	/// set at all, just use the minimum possible lookahead to figure out whether 
	/// the non-exit arms could possibly match."
	/// <para/>
	/// By the way, if the follow set is <c>.* $</c> and you use a nongreedy()
	/// loop, the loop will never execute since <c>.* $</c> matches anything and
	/// is always preferred.
	/// <para/>
	/// Here's an example that needs more than one character of lookahead:
	/// <code>
	/// // public rule Foo ==> #[ 'a'..'z'+ | 'x' '0'..'9' '0'..'9' ];
	/// public void Foo()
	/// {
	///   int la0, la1;
	///   la0 = LA(0);
	///   int alt = 0;
	///   if (la0 == 'x') {
	///     la1 = LA(1);
	///     if (la1 >= '0' && '9' >= la1) {
	///       Match();
	///       Match();
	///       MatchRange('0', '9');
	///     } else
	///       alt = 1;
	///   } else
	///     alt = 1;
	/// 
	///   if (alt == 1) {
	///     Match();
	///     for (;;) {
	///       la0 = LA(0);
	///       if (la0 >= 'a' && 'z' >= la0)
	///         Match();
	///       else
	///         break;
	///     }
	///   }
	/// }
	/// </code>
	/// Here, the prediction and matching phases are merged for the second 
	/// alternative, but separate for the first alternative (because it is chosen 
	/// in two different places in the prediction logic). Notice that the matching 
	/// for alt 2 starts with <c>Match()</c> twice, with no arguments, but is 
	/// followed by <c>MatchRange('a', 'z')</c>. This demonstrates communication 
	/// between prediction and matching: the matching phase can tell that LA(0) is 
	/// confirmed to be 'x', and LA(1) is confirmed to be '0'..'9', so an 
	/// unconditional match suffices. However, nothing is known about LA(2) so its 
	/// value must be checked, which is what MatchRange() is supposed to do.
	/// <para/>
	/// In some cases, LA(0) is irrelevant. Consider this example:
	/// <code>
	/// // public rule Foo ==> #[ '(' 'a'..'z'* ')' | '(' '0'..'9'+ ')' ];
	/// public void Foo()
	/// {
	///   int la0, la1;
	///   la1 = LA(1);
	///   if (la1 >= 'a' && 'z' >= la1) {
	///     Match('(');
	///     for (;;) {
	///       la0 = LA(0);
	///       if (la0 >= 'a' && 'z' >= la0)
	///         Match();
	///       else
	///         break;
	///     }
	///     Match(')');
	///   } else {
	///     Match('(');
	///     MatchRange('0', '9');
	///     for (;;) {
	///       la0 = LA(0);
	///       if (la0 >= '0' && '9' >= la0)
	///         Match();
	///       else
	///         break;
	///     }
	///     Match(')');
	///   }
	/// }
	/// </code>
	/// Here, the first character of both alternatives is always '(', so looking at
	/// LA(0) doesn't help choose which branch to take, and prediction skips ahead
	/// to LA(1).
	/// </remarks>
	public class LLParserGenerator : PGFactory
	{
		#region Tests
		void Seq()
		{
			
		}
		#endregion

		Dictionary<Symbol, Rule> _rules;
		HashSet<Rule> _tokens = new HashSet<Rule>();
		public int DefaultK = 8;
		public int FollowSetK = 2;
		public int TokenFollowSetK = 1;

		#region Step 1: AddRules() and related

		public LLParserGenerator() { }

		public Dictionary<Symbol, Rule> AddRules(Node stmtList)
		{
			_rules = new Dictionary<Symbol, Rule>();
			foreach (var stmt in stmtList.Args)
			{
				if (stmt.Calls(S.Def, 4))
				{
					bool isToken;
					var name = stmt.Args[0].Name;
					if ((isToken = name == _token) || stmt.Args[0].Name == _rule) try
						{
							var body = stmt.Args[3];
							var expr = body.Args[body.Args.Count - 1];
							var rule = new Rule(expr, name, NodeToPred(expr), stmt.TryGetAttr(S.Public) != null) { IsToken = isToken };
							AddRule(rule);
						}
						catch (Exception ex)
						{
							Console.WriteLine("ConvertRule failed: " + ex.Message);
						}
				}
			}
			return _rules;
		}
		public void AddRules(IEnumerable<Rule> rules)
		{
			foreach (var rule in rules)
				AddRule(rule);
		}
		public void AddRule(Rule rule)
		{
			_rules.Add(rule.Name, rule);
			if (rule.IsToken)
				_tokens.Add(rule);
		}

		enum Context { Rule, GateLeft, GateRight, And };

		private Pred NodeToPred(Node expr, Context ctx = Context.Rule)
		{
			if (expr.IsCall)
			{
				bool slash = false, not, orTerminals;
				if (expr.Calls(S.DotDot, 2) && expr.Args[0].IsLiteral && expr.Args[1].IsLiteral)
				{
					object v0 = expr.Args[0].Value, v1 = expr.Args[1].Value;
					if (v0 is char && v1 is char)
						return TerminalSet.New(expr, (char)v0, (char)v1);
				}
				else if (expr.CallsMin(S.Tuple, 1))
				{
					// sequence: (a, b, c)
					if (expr.Calls(S.Tuple, 1))
						return NodeToPred(expr.Args[0], ctx);
					return ArgsToSeq(expr, ctx);
				}
				else if ((orTerminals = expr.Calls(S.OrBits, 2)) || expr.Calls(S.Or, 2) || (slash = expr.Calls(S.Div, 2)))
				{
					// alternatives: a | b, a || b, a / b
					var left = NodeToPred(expr.Args[0], ctx);
					var right = NodeToPred(expr.Args[1], ctx);
					var lt = AsTerminalSet(left);
					var rt = AsTerminalSet(right);
					if (lt != null && rt != null && lt.CanMerge(rt))
						return lt.Union(rt);
					else
					{
						//if (orTerminals)
						//	throw new ArgumentException("Cannot use '{0}' as an argument to '|' because it is not a terminal set. Use '||' instead.",
						//		expr.Args[lt != null ? 1 : 0].ToString());
						return new Alts(expr, left, right, slash);
					}
				}
				else if (expr.CallsMin(_Star, 1) || expr.CallsMin(_Plus, 1) || expr.CallsMin(_Opt, 1))
				{
					// loop (a`+`, a`*`) or optional (a`?`)
					var type = expr.Name;
					bool greedy = true;
					Pred subpred = null;
					if (expr.ArgCount == 1)
					{ // +, * or ? had only one argument (usual case)
						expr = expr.Args[0];
						if ((greedy = expr.CallsMin(_Greedy, 1)) || expr.CallsMin(_Nongreedy, 1))
						{
							slash = true; // ignore ambiguous
							if (expr.Args.Count == 1)
								subpred = NodeToPred(expr.Args[0], ctx);
							else
								subpred = ArgsToSeq(expr, ctx);
						}
						else
							subpred = NodeToPred(expr, ctx);
					}
					else
					{
						subpred = ArgsToSeq(expr, ctx);
					}

					if (type == _Opt)
						return new Alts(expr, LoopMode.Opt, subpred) { Greedy = greedy };
					if (type == _Plus)
					{
						var seq = new Seq(expr);
						seq.List.Add(subpred);
						seq.List.Add(new Alts(expr, LoopMode.Star, subpred));
						return seq;
					}
					return new Alts(expr, LoopMode.Star, subpred) { Greedy = greedy };
				}
				else if (expr.Calls(_Gate, 2))
				{
					if (ctx == Context.GateLeft || ctx == Context.GateRight)
						throw new ArgumentException(Localize.From(
							"Cannot use a gate ('{0}') inside another gate", expr));
					return new Gate(expr, NodeToPred(expr.Args[0], Context.GateLeft),
										  NodeToPred(expr.Args[1], Context.GateRight));
				}
				else if ((not = expr.Calls(S.Not, 1)) || expr.Calls(S.AndBits, 1))
				{
					expr = expr.Args[0];
					var subpred = AutoNodeToPred(expr, Context.And);
					var subpred2 = subpred as AndPred;
					if (subpred2 != null)
					{
						subpred2.Not ^= not;
						return subpred2;
					}
					else
						return new AndPred(expr, subpred, not);
				}
			}
			else
			{
				// Non-call
				while (expr.Head != null) // eliminate parenthesis
					expr = expr.Head;
				if (expr.IsLiteral && expr.Value is char)
					return TerminalSet.New(expr, (char)expr.Value);
				if (expr.IsSimpleSymbol)
					return new RuleRef(expr, _rules[expr.Name]);
			}
			throw new ArgumentException("Unrecognized expression '{0}'", expr.ToString());
		}
		private Seq ArgsToSeq(Node expr, Context ctx)
		{
			var objs = expr.Args.Select(node => AutoNodeToPred(node, ctx)).ToList();
			Seq seq = new Seq(expr);
			Node action = null;
			for (int i = 0; i < objs.Count; i++)
			{
				if (objs[i] is Node)
				{
					var code = objs[i] as Node;
					if (ctx == Context.And || ctx == Context.GateLeft)
						throw new ArgumentException(Localize.From(ctx == Context.And ?
							"Cannot use an action block ('{0}') inside an '&' or '!' predicate; these predicates are for prediction only." :
							"Cannot use an action block ('{0}') on the left side of a '=>' gate; the left side is for prediction only.", objs[i].ToString()));
					action = Pred.AppendAction(action, code);
				}
				else // Pred
				{
					Pred pred = (Pred)objs[i];
					pred.PreAction = action;
					action = null;
					seq.List.Add(pred);
				}
			}
			if (action != null)
				seq.PostAction = action;
			return seq;
		}
		private object AutoNodeToPred(Node expr, Context ctx)
		{
			if (expr.CallsMin(S.Braces, 0))
				return expr; // code
			return NodeToPred(expr, ctx);
		}
		static TerminalSet AsTerminalSet(Pred pred)
		{
			if (pred is RuleRef)
				return AsTerminalSet(((RuleRef)pred).Rule.Pred);
			if (pred is TerminalSet)
				return (TerminalSet)pred;
			return null;
		}

		#endregion

		#region Step 2: DetermineFollowSets() and related

		void DetermineFollowSets()
		{
			foreach (Rule rule in _rules.Values)
				new DetermineLocalFollowSets().Run(rule);

			// Each rule's Next is always an EndOfRule object, which has a list 
			// of things that could follow the rule elsewhere in the grammar.
			// To determine the follow set of each rule, me must find all places 
			// where the rule is used...
			new DetermineRuleFollowSets(_rules).Run();
		}

		class DetermineLocalFollowSets : PredVisitor
		{
			AnyTerminal AnyFollowSet = AnyTerminal.AnyFollowSet();

			public void Run(Rule rule)
			{
				Visit(rule.Pred, rule.EndOfRule);
			}
			void Visit(Pred pred, Pred next)
			{
				pred.Next = next;
				pred.Call(this);
			}

			public override void Visit(Seq seq)
			{
				var next = seq.Next;
				for (int i = seq.List.Count - 1; i >= 0; i--)
				{
					Visit(seq.List[i], next);
					next = seq.List[i];
				}
			}
			public override void Visit(Alts alts)
			{
				for (int i = 0; i < alts.Arms.Count; i++)
					Visit(alts.Arms[i], alts.Next);
			}
			public override void Visit(Gate gate)
			{
				Visit(gate.Match, gate);
				Visit(gate.Predictor, AnyFollowSet);
			}
			public override void Visit(AndPred pred)
			{
				var child = pred.Pred as Pred;
				if (child != null)
					Visit(child, AnyFollowSet);
			}
		}

		class DetermineRuleFollowSets : RecursivePredVisitor
		{
			private Dictionary<Symbol, Rule> _rules;
			public DetermineRuleFollowSets(Dictionary<Symbol, Rule> rules) { _rules = rules; }

			public void Run()
			{
				foreach (Rule rule in _rules.Values)
					rule.Pred.Call(this);
			}
			public override void Visit(RuleRef rref)
			{
				if (rref.Next is EndOfRule)
					rref.Rule.EndOfRule.FollowSet.UnionWith((rref.Next as EndOfRule).FollowSet);
				else
					rref.Rule.EndOfRule.FollowSet.Add(rref.Next);
			}
		}

		#endregion

		public Node GenerateCode(Symbol className, ISourceFile sourceFile)
		{
			DetermineFollowSets();

			var F = new GreenFactory(sourceFile);
			// TODO use class body provided by user
			var greenClass = F.Call(S.Class, F.Symbol(className), F.List(), F.Braces());
			var result = Node.NewCursorFromGreen(greenClass, -1);

			var generator = new GenerateCodeVisitor(new NodeFactory(sourceFile)) { Body = result.Args[3] };
			foreach(var rule in _rules.Values) {
				generator.CurrentRule = rule;
				generator.Visit(rule.Pred);
			}
			return result;
		}

		class GenerateCodeVisitor : RecursivePredVisitor
		{
			public Node Body;
			public Rule CurrentRule;
			public NodeFactory F;
			GreenFactory GF;
			Node _backupBasis;

			public static readonly Symbol _alt = GSymbol.Get("alt");

			public GenerateCodeVisitor(NodeFactory f)
			{
				F = f;
				GF = new GreenFactory(f.File);
				_backupBasis = CompilerCore.Node.NewSynthetic(GSymbol.Get("nowhere"), new SourceRange(F.File, -1, -1));
			}

			public override void Visit(Alts alts)
			{
				// FOR NOW, LET'S IGNORE AndPred!
				Generate(alts, Body);
			}

			Node Node(GreenNode gnode, Node forSourceIndex)
			{
				return Loyc.CompilerCore.Node.NewFromGreen(gnode, (forSourceIndex ?? _backupBasis).SourceIndex);
			}

			void Generate(Alts alts, Node block)
			{
				var firstSets = ComputeFirstSets(alts);
				TerminalSet covered = TerminalSet.Empty;
				var thisBranch = new List<Pair<TerminalSet,int>>();
				var predictionTable = new List<Pair<TerminalSet, Node>>();
				
				// A list of statements to run once prediction is complete. By 
				// default it's just "alt = i" (or "break" for the exit branch)
				var handlers = new List<Node>();
				int i;
				for (i = 0; i < alts.Arms.Count; i++)
					handlers.Add(Node(GF.Call(S.Set, GF.Symbol(_alt), GF.Literal(i+1)), alts.Arms[i].Basis));
				if (alts.Mode != LoopMode.None)
					handlers.Add(Node(GF.Symbol(S.Break), alts.Basis));

				// Compute the overlap between different first sets...
				for(;;) {
					TerminalSet set = null;
					for (i = 0; ; i++) {
						if (i == firstSets.Length)
							return; // done!
						set = firstSets[i].A.Subtract(covered);
						if (!set.IsEmptySet)
							break;
					}
					
					thisBranch.Add(G.Pair(set, firstSets[i].B));
					for (i++; i < firstSets.Length; i++) {
						var next = set.Intersection(firstSets[i].A);
						if (!next.IsEmptySet) {
							set = next;
							thisBranch.Add(firstSets[i]);
						}
					}

					if (thisBranch.Count == 1) {
						predictionTable.Add(G.Pair(set, handlers[thisBranch[0].B]));
					} else {
						for (i = 0; i < thisBranch.Count; i++)
							thisBranch[i] = G.Pair(thisBranch[i].A.Intersection(set), thisBranch[i].B);

						throw new NotImplementedException();//TODO
					}
				}
			}

			private Pair<TerminalSet,int>[] ComputeFirstSets(Alts alts)
			{
				bool hasExit = alts.Mode != LoopMode.None;
				var firstSets = new Pair<TerminalSet,int>[alts.Arms.Count + (hasExit ? 1 : 0)];

				int i;
				for (i = 0; i < alts.Arms.Count; i++)
					firstSets[i] = G.Pair(ComputeFirstSet(alts.Arms[i]), i);
				if (hasExit)
					firstSets[i] = G.Pair(ComputeFirstSet(alts.Next), -1);
				if ((uint)alts.DefaultArm < (uint)alts.Arms.Count)
					InternalList.Move(firstSets, alts.DefaultArm, i);
				return firstSets;
			}

			private TerminalSet ComputeFirstSet(Pred pred)
			{
				throw new NotImplementedException();
			}

			public override void Visit(Seq pred)
			{
				VisitChildrenOf(pred);
			}
			public override void Visit(Gate pred)
			{
				Visit(pred.Match);
			}
			public override void Visit(AndPred pred)
			{
				// ignore, for now
			}
		}
	}

	public class Rule
	{
		public readonly Node Basis;
		public readonly EndOfRule EndOfRule = new EndOfRule();

		public Rule(Node basis, Symbol name, Pred pred, bool isStartingRule) 
		{
			Basis = basis; Pred = pred; Name = name;
			if (IsStartingRule = isStartingRule)
				EndOfRule.FollowSet.Add(new CharSetTerminal(null, new IntRange(-1)));
		}
		public readonly Symbol Name;
		public readonly Pred Pred;
		public bool IsToken, IsStartingRule;

		public static Alts operator |(Rule a, Pred b) { return (RuleRef)a | b; }
		public static Alts operator |(Pred a, Rule b) { return a | (RuleRef)b; }
		public static Alts operator |(Rule a, Rule b) { return (RuleRef)a | (RuleRef)b; }
		public static implicit operator Rule(RuleRef rref) { return rref.Rule; }
		public static implicit operator RuleRef(Rule rule) { return new RuleRef(null, rule); }
	}

	/*
	 * token Id() {
	 *   (('@', {_verbatim=1;})`?`, NormalIdStart, NormalIdCont`*`) |
	 *   ('@', {_verbatim=1;}, SqString);
	 * }
	 * rule IdStart() { Letter | '_'; }
	 * rule IdCont()  { IdStart | '0'..'9'; }
	 * rule Letter()  { ('a'..'z') | ('A'..'Z') | (&{Char.IsLetter(LA0)}, _); }
	 * 
	 * 
	 * 
	 * rule goo() => { foo 'z' }
	 * rule foo() => { nongreedy(('a' | 'b') num | 'c')* ('c' | 'd') | 'a'+; }
	 * rule num() => { ('0'..'9')+; }
	 * 
	 * static readonly InputSet num__set0 = Range('0','9');
	 * void foo()
	 * {
	 *   int alt;
	 *   char LA0, LA1;
	 *   LA0 = LA(0);
	 *   if (LA0 == 'a') {
	 *     LA1 = LA(1);
	 *     if (LA1 >= '0' && LA1 <= '9')
	 *       alt = 0;
	 *     else
	 *       alt = 1;
	 *   } else
	 *     alt = 1;
	 *   
	 *   if (alt == 0) {
	 *     for (;;) { // for `?` use do...while(false)
	 *       LA0 = LA(0);
	 *       if (LA0 >= 'a' && LA0 <= 'b')
	 *         alt = 0;
	 *       else if (LA0 == 'c') {
	 *         if (LA0 == 'z')
	 *           break;
	 *         else
	 *           alt = 1;
	 *       } else
	 *         break;
	 *       
	 *       if (alt == 0) {
	 *         Consume(); // alt == 0 already implies 'a'..'b'; no check needed
	 *         num();
	 *       }
	 *     }
	 *   } else {
	 *     Consume();
	 *   }
	 * }
	 * void num()
	 * {
	 *   int alt;
	 *   char LA0;
	 *   LA0 = LA(0);
	 *   Match(LA0 >= '0' && LA0 <= '9', "0..9");
	 *   for (;;) {
	 *     LA0 = LA(0);
	 *     if (LA0 >= '0' && LA0 <= '9')
	 *       alt = 0;
	 *     else
	 *       alt = -1;
	 *     if (alt == 0)
	 *       Consume();
	 *     else
	 *       break;
	 *   }
	 * }
	 * 
	 * 
	 * Fun time! So hey, what would this less-ambiguous C alternative look like? D?
	 * - Juxtaposition operator is not possible in general because x - y, x `@` y
	 *   would be ambiguous: could be (x) (-y), (x `@`) (y)
	 * - In boo style, can allow arbitrary macro names without parens e.g. 
	 *       boo.foo (bar) - 1: ... 
	 *   Or if braces normally start child blocks:
	 *       boo.foo (bar) - 1 { ... }
	 *   In that case, need something else like {{ }} to make a scope mid-statement.
	 *   presence of ':' indicates that 'boo.foo' must be a macro name;
	 *   "assert (x) > (y)" can't work this way, but "assert: (x) > (y)" can.
	 *   Labels would need some other syntax such as 
	 * 
	 * Distinguishing argument lists from expressions: arg list when
	 * 1. beginning of statement, multiple words before parens, and/or
	 * 2. parens followed by =>
	 * a b c d(e < f, g > h, j < k, l < m >> m) => { ... }
	 * Note: constructors and destructors may look like exprs; assume fn call
	 *       and disambiguate in postprocessing step.
	 * Note: var decl in expression requires '=' to disambiguate, either right
	 *       after the type and name (int x = ...) or outside the parenthesis 
	 *       (x, int y, a[i]) = ...
	 */

}