﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Loyc.Syntax;
using Loyc.Collections;
using S = Loyc.Syntax.CodeSymbols;

namespace Loyc.LLParserGenerator
{

	/// <summary>
	/// A class that implements this interface will generate small bits of code 
	/// that the parser generator will use. The default implementation is
	/// <see cref="IntStreamCodeGenHelper"/>. To install a new code generator,
	/// set the <see cref="LLParserGenerator.SnippetGenerator"/> property or
	/// supply the generator in the constructor of <see cref="LLParserGenerator"/>.
	/// </summary>
	public interface IPGCodeGenHelper
	{
		/// <summary>Returns an empty set of the appropriate type for the kind of 
		/// parser being generated by this code.</summary>
		IPGTerminalSet EmptySet { get; }

		/// <summary>Simplifies the specified set, if possible, so that GenerateTest() 
		/// can generate simpler code for an if-else chain in a prediction tree.</summary>
		/// <param name="dontcare">A set of terminals that have been ruled out,
		/// i.e. it is already known that the lookahead value is not in this set.</param>
		/// <returns>An optimized set, or this.</returns>
		IPGTerminalSet Optimize(IPGTerminalSet set, IPGTerminalSet dontcare);

		/// <summary>Returns an example of a character in the set, or null if this 
		/// is not a set of characters or if EOF is the only member of the set.</summary>
		/// <remarks>This helps produce error messages in LLLPG.</remarks>
		char? ExampleChar(IPGTerminalSet set);
		/// <summary>Returns an example of an item in the set. If the example is
		/// a character, it should be surrounded by single quotes.</summary>
		/// <remarks>This helps produce error messages in LLLPG.</remarks>
		string Example(IPGTerminalSet set);

		/// <summary>Before the parser generator generates code, it calls this
		/// method.</summary>
		/// <param name="classBody">the body (braced block) of the class where 
		/// the code will be generated, which allows the snippet generator to add 
		/// code at class level when needed.</param>
		/// <param name="sourceFile">the suggested <see cref="ISourceFile"/> to 
		/// assign to generated code snippets.</param>
		void Begin(RWList<LNode> classBody, ISourceFile sourceFile);

		/// <summary>Notifies the snippet generator that code generation is 
		/// starting for a new rule.</summary>
		void BeginRule(Rule rule);

		/// <summary><see cref="LLParserGenerator"/> calls this method to notify
		/// the snippet generator that code generation is complete.</summary>
		void Done();

		/// <summary>Generate code to match any token.</summary>
		/// <returns>Default implementation returns <c>@{ Skip(); }</c>, or 
		/// @{ MatchAny(); } if the result is to be saved.</returns>
		LNode GenerateSkip(bool savingResult);

		/// <summary>Generate code to check the result of an and-predicate 
		/// during or after prediction (the code to test the and-predicate has
		/// already been generated and is passed in as the 'code' parameter),
		/// e.g. &!{foo} typically becomes !(foo) during prediction and 
		/// Check(!(foo), "foo"); afterward.</summary>
		/// <param name="andPred">Predicate for which to generate code</param>
		/// <param name="code">The code of the predicate, which is basically 
		/// <c>(andPred.Pred as Node)</c> or some other expression generated 
		/// based on <c>andPred.Pred</c>.</param>
		/// <param name="predict">true to generate prediction code, false for checking post-prediction</param>
		LNode GenerateAndPredCheck(AndPred andPred, LNode code, bool predict);

		/// <summary>Generate code to match a set, e.g. 
		/// <c>@{ MatchRange('a', 'z');</c> or <c>@{ MatchExcept('\n', '\r'); }</c>.
		/// If the set is too complex, a declaration for it is created in classBody.</summary>
		LNode GenerateMatch(IPGTerminalSet set_, bool savingResult, bool recognizerMode);

		/// <summary>Generates code to read LA(k).</summary>
		/// <returns>The default implementation returns @(LA(k)).</returns>
		LNode LA(int k);

		/// <summary>Returns the data type of LA(k)</summary>
		/// <returns>The default implementation returns @(int).</returns>
		LNode LAType();

		/// <summary>Generates code for the error branch of prediction.</summary>
		/// <param name="currentRule">Rule in which the code is generated.</param>
		/// <param name="covered">The permitted token set, which the input did not match. 
		/// NOTE: if the input matched but there were and-predicates that did not match,
		/// this parameter will be null (e.g. the input is 'b' in <c>(&{x} 'a' | &{y} 'b')</c>,
		/// but y is false.</param>
		LNode ErrorBranch(IPGTerminalSet covered, int laIndex);

		/// <summary>Returns true if a "switch" statement is the preferable code 
		/// generation technique rather than the default if-else chain</summary>
		/// <param name="branchSets">Non-overlapping terminal sets, one set for each 
		/// branch of the prediction tree.</param>
		/// <param name="casesToInclude">To this set, this method should add the 
		/// indexes of branches for which case labels should be generated, e.g.
		/// adding index 2 means that switch cases should be generated for sets[2].
		/// The caller (<see cref="LLParserGenerator"/>) will create an if-else 
		/// chain for all branches that are not added to casesToInclude, and this 
		/// chain will be passed to <see cref="GenerateSwitch"/>.</param>
		/// <remarks>
		/// Using a switch() statement can be important for performance, since the
		/// compiler may be able to implement a switch statement using as little as
		/// a single branch, unlike an if-else chain which often requires multiple
		/// branches.
		/// <para/>
		/// However, it does not always make sense to use switch(), and when it does 
		/// make sense, it may not be wise or possible to include all cases in the
		/// switch, so this method is needed to make the decision.
		/// <para/>
		/// Consider an example with four branches, each having a character set, 
		/// plus an error branch:
		/// <pre>
		///     Branch 1: '*'|'+'|'-'|'/'|'%'|'^'|'&'|','|'|'
		///     Branch 2: '_'|'$'|'a'..'z'|'A'..'Z'|128..65535
		///     Branch 3: '0'..'9'
		///     Branch 4: ' '|'\t'
		///     Error: anything else
		/// </pre>
		/// In this case, it is impossible (well, quite impractical) to use cases 
		/// for all of Branch 2. The most sensible switch() statement probably looks 
		/// like this:
		/// <pre>
		///     switch(la0) {
		///     case '*': case '+': case '-': case '/': case '%':
		///     case '^': case '&': case ',': case '|':
		///         // branch 1
		///     case '0': case '1': case '2': case '3': case '4': 
		///     case '5': case '6': case '7': case '8': case '9': 
		///         // branch 3
		///     case ' ': case '\t':
		///         // branch 4
		///     default:
		///         if (la0 >= 'A' && la0 &lt;= 'Z' || la0 >= 'a' && la0 &lt;= 'z' || la0 >= 128 && la0 &lt;= 65536)
		///             // branch 2
		///         else
		///             // error
		///     }
		/// </pre>
		/// Please note that given LLLPG's current design, it is not possible to "split" a 
		/// branch. For example, the switch cannot include "case '_': case '$':" and use this
		/// to handle branch 2 (but not the error case), while also handling branch 2 in the
		/// "default" case. Although LLLPG has a mechanism to duplicate branches of an 
		/// <see cref="Alts"/> so that the code for handling an alternative is located at 
		/// two different places in a prediction tree (using 'goto' if necessary), it does 
		/// not have a similar mechanism for arbitrary subtrees of a prediction tree.
		/// <para/>
		/// 'sets' does not include the error branch, if any. If there's no error branch, the
		/// last case should be left out of 'casesToInclude' so that there will be a 
		/// 'default:' case. Note: it should always be the <i>last</i> set that is left
		/// out, because that will be the official default branch (the user can control
		/// which branch is default, hence which one comes last, using the 'default' keyword
		/// in the grammar DSL.)
		/// </remarks>
		bool ShouldGenerateSwitch(IPGTerminalSet[] sets, MSet<int> casesToInclude, bool hasErrorBranch);

		/// <summary>Generates a switch statement with the specified branches where
		/// branchCode[i] is the code to run if the input is in the set branchSets[i].</summary>
		/// <param name="casesToInclude">The set chosen by <see cref="ShouldGenerateSwitch"/>.</param>
		/// <param name="defaultBranch">Code to be placed in the default: case (if none, the blank stmt <c>@``;</c>)</param>
		/// <param name="laVar">The lookahead variable being switched on (e.g. la0)</param>
		/// <returns>The generated switch block.</returns>
		LNode GenerateSwitch(IPGTerminalSet[] branchSets, MSet<int> casesToInclude, LNode[] branchCode, LNode defaultBranch, LNode laVar);

		/// <summary>Generates code to test whether the terminal denoted 'laVar' is in the set.</summary>
		LNode GenerateTest(IPGTerminalSet set, LNode laVar);

		/// <summary>Generates the method for a rule, given the method's contents.</summary>
		/// <param name="rule">Rule for which a method is needed.</param>
		/// <param name="methodBody">A list of statements produced by 
		/// LLParserGenerator inside the method.</param>
		/// <param name="recognizerMode">If true, the rule is a recognizer (a 
		/// lookahead helper for &(syntactic predicates)), which means it will
		/// need a 'bool' return value and a 'return true' statement added to
		/// the end.</param>
		/// <returns>A method definition for the rule.</returns>
		/// <remarks>To generate the default method, simply call 
		/// <c>rule.CreateMethod(methodBody, recognizerMode)</c></remarks>
		LNode CreateRuleMethod(Rule rule, RVList<LNode> methodBody, bool recognizerMode);

		/// <summary>Generates code to call a rule based on <c>rref.Rule.Name</c>
		/// and <c>rref.Params</c>.</summary>
		/// <returns>Should return <c>rref.AutoSaveResult(code)</c> where 
		/// <c>code</c> is the code to invoke the rule.</returns>
		LNode CallRuleAndSaveResult(RuleRef rref);
	}


	/// <summary>Suggested base class for custom code generators. Each derived 
	/// class is typically designed for a different kind of token.
	/// <remarks>
	/// LLPG comes with two derived classes, <see cref="PGCodeGenForIntStream"/> 
	/// for parsing input streams of characters or integers, and 
	/// <see cref="PGCodeGenForSymbolStream"/> for parsing streams of 
	/// <see cref="Symbol"/>s.
	/// </remarks>
	public abstract class CodeGenHelperBase : IPGCodeGenHelper
	{
		protected static readonly Symbol _Skip = GSymbol.Get("Skip");
		protected static readonly Symbol _MatchAny = GSymbol.Get("MatchAny");
		protected static readonly Symbol _Match = GSymbol.Get("Match");
		protected static readonly Symbol _MatchExcept = GSymbol.Get("MatchExcept");
		protected static readonly Symbol _MatchRange = GSymbol.Get("MatchRange");
		protected static readonly Symbol _MatchExceptRange = GSymbol.Get("MatchExceptRange");
		protected static readonly Symbol _TryMatch = GSymbol.Get("TryMatch");
		protected static readonly Symbol _TryMatchExcept = GSymbol.Get("TryMatchExcept");
		protected static readonly Symbol _TryMatchRange = GSymbol.Get("TryMatchRange");
		protected static readonly Symbol _TryMatchExceptRange = GSymbol.Get("TryMatchExceptRange");
		protected static readonly Symbol _Check = GSymbol.Get("Check");

		protected int _setNameCounter = 0;
		protected LNodeFactory F;
		protected RWList<LNode> _classBody;
		protected Rule _currentRule;
		Dictionary<IPGTerminalSet, Symbol> _setDeclNames;

		public virtual void Begin(RWList<LNode> classBody, ISourceFile sourceFile)
		{
			_classBody = classBody;
			F = new LNodeFactory(sourceFile);
			_setDeclNames = new Dictionary<IPGTerminalSet, Symbol>();
		}
		public virtual void BeginRule(Rule rule)
		{
			_currentRule = rule;
			_setNameCounter = 0;
		}
		public virtual void Done()
		{
			_classBody = null;
			F = null;
			_setDeclNames = null;
			_currentRule = null;
		}

		public virtual LNode GenerateTest(IPGTerminalSet set, LNode laVar)
		{
			LNode test = GenerateTest(set, laVar, null);
			if (test == null) {
				var setName = GenerateSetDecl(set);
				test = GenerateTest(set, laVar, setName);
			}
			return test;
		}

		/// <summary>Generates code to test whether a terminal is in the set.</summary>
		/// <param name="subject">Represents the variable to be tested.</param>
		/// <param name="setName">Names an external set variable to use for the test.</param>
		/// <returns>A test expression such as @(la0 >= '0' && '9' >= la0), or 
		/// null if an external setName is needed and was not provided.</returns>
		/// <remarks>
		/// At first, <see cref="LLParserGenerator"/> calls this method with 
		/// <c>setName == null</c>. If it returns null, it calls the method a
		/// second time, giving the name of an external variable in which the
		/// set is held (see <see cref="GenerateSetDecl"/>).
		/// <para/>
		/// For example, if the subject is @(la0), the test for a simple set
		/// like [a-z?] might be something like <c>@((la0 >= 'a' && 'z' >= la0)
		/// || la0 == '?')</c>. When the setName is @(foo), the test might be 
		/// <c>@(foo.Contains(la0))</c> instead.
		/// </remarks>
		protected abstract LNode GenerateTest(IPGTerminalSet set, LNode subject, Symbol setName);

		protected virtual Symbol GenerateSetName(Rule currentRule)
		{
			return GSymbol.Get(string.Format("{0}_set{1}", currentRule.Name.Name, _setNameCounter++));
		}

		protected virtual Symbol GenerateSetDecl(IPGTerminalSet set)
		{
			Symbol setName;
			if (_setDeclNames.TryGetValue(set, out setName))
				return setName;

			setName = GenerateSetName(_currentRule);
			_classBody.Add(GenerateSetDecl(set, setName));

			return _setDeclNames[set] = setName;
		}

		/// <summary>Generates a declaration for a variable that holds the set.</summary>
		/// <remarks>
		/// For example, if setName is foo, a set such as [aeiouy] 
		/// might use an external declaration such as 
		/// <code>IntSet foo = IntSet.Parse("[aeiouy]");</code>
		/// This method will not be called if <see cref="GenerateTest(Node)"/>
		/// never returns null.
		/// </remarks>
		protected abstract LNode GenerateSetDecl(IPGTerminalSet set, Symbol setName);

		/// <summary>Returns <c>@{ Skip(); }</c>, or @{ MatchAny(); } if the result 
		/// is to be saved.</summary>
		public virtual LNode GenerateSkip(bool savingResult) // match anything
		{
			if (savingResult)
				return F.Call(_MatchAny);
			else
				return F.Call(_Skip);
		}

		/// <summary>Generate code to check an and-predicate during or after prediction, 
		/// e.g. &!{foo} becomes !(foo) during prediction and Check(!(foo)); afterward.</summary>
		/// <param name="andPred">Predicate for which an expression has already been generated</param>
		/// <param name="andPred">The expression to be checked</param>
		/// <param name="predict">true to generate prediction expr, false for checking post-prediction</param>
		public virtual LNode GenerateAndPredCheck(AndPred andPred, LNode code, bool predict)
		{
			code = code.Clone(); // in case it's used more than once
			if (andPred.Not)
				code = F.Call(S.Not, code);
			if (predict)
				return code;
			else {
				string asString = (andPred.Pred is LNode 
					? ((LNode)andPred.Pred).Print(NodeStyle.Expression) 
					: andPred.Pred.ToString());
				return F.Call(_Check, code, F.Literal(asString));
			}
		}

		/// <summary>Generate code to match a set, e.g. 
		/// <c>@{ MatchRange('a', 'z');</c> or <c>@{ MatchExcept('\n', '\r'); }</c>.
		/// If the set is too complex, a declaration for it is created in classBody.</summary>
		public abstract LNode GenerateMatch(IPGTerminalSet set_, bool savingResult, bool recognizerMode);

		protected readonly Symbol _LA = GSymbol.Get("LA");
		protected readonly Symbol _LA0 = GSymbol.Get("LA0");

		/// <summary>Generates code to read LA(k).</summary>
		/// <returns>Default implementation returns LA0 for k==0, LA(k) otherwise.</returns>
		public virtual LNode LA(int k)
		{
			return k == 0 ? F.Id(_LA0) : F.Call(_LA, F.Literal(k));
		}

		/// <summary>Generates code for the error branch of prediction.</summary>
		/// <param name="currentRule">Rule in which the code is generated.</param>
		/// <param name="covered">The permitted token set, which the input did not match. 
		/// NOTE: if the input matched but there were and-predicates that did not match,
		/// this parameter will be null (e.g. the input is 'b' in <c>(&{x} 'a' | &{y} 'b')</c>,
		/// but y is false.</param>
		public virtual LNode ErrorBranch(IPGTerminalSet covered, int laIndex)
		{
			string coveredS = covered.ToString();
			if (coveredS.Length > 45)
				coveredS = coveredS.Substring(0, 40) + "...";
			return F.Call("Error", F.Call(S.Add, F.Id("InputPosition"), F.Literal(laIndex)), 
				F.Literal(string.Format("In rule '{0}', expected one of: {1}", _currentRule.Name.Name, coveredS)));
		}

		/// <summary>Returns the data type of LA(k)</summary>
		/// <returns>Default implementation returns @(int).</returns>
		public abstract LNode LAType();

		public abstract IPGTerminalSet EmptySet { get; }
		public virtual IPGTerminalSet Optimize(IPGTerminalSet set, IPGTerminalSet dontcare) { return set.Subtract(dontcare); }
		public virtual char? ExampleChar(IPGTerminalSet set) { return null; }
		public abstract string Example(IPGTerminalSet set);

		/// <summary>Used to help decide whether a "switch" or an if-else chain 
		/// will be used for prediction. This is the starting cost of a switch 
		/// (the starting cost of an if-else chain is set to zero).</summary>
		protected virtual int BaseCostForSwitch { get { return 8; } }
		/// <summary>Used to help decide whether a "switch" or an if statement
		/// will be used to handle a prediction tree, and if so which branches.
		/// This method should calculate the "cost of switch" (which generally 
		/// represents a code size penalty, as there is a separate case for 
		/// every element of the set) and the "cost of if" (which generally 
		/// represents a speed penalty) and return the difference (so that 
		/// positive numbers favor "switch" and negative numbers favor "if".)</summary>
		/// <remarks>If the set is inverted, return a something like -1000000 
		/// to ensure 'switch' is not used for that set.</remarks>
		protected virtual int GetRelativeCostForSwitch(IPGTerminalSet set) { return -1000000; }
		/// <summary>Gets the literals or symbols to use for switch cases of
		/// a set (just the values, not including the case labels.)</summary>
		protected virtual IEnumerable<LNode> GetCases(IPGTerminalSet set) { throw new NotImplementedException(); }

		/// <summary>Decides whether to use a switch() and for which cases, using
		/// <see cref="BaseCostForSwitch"/> and <see cref="GetRelativeCostForSwitch"/>.</summary>
		public virtual bool ShouldGenerateSwitch(IPGTerminalSet[] sets, MSet<int> casesToInclude, bool hasErrorBranch)
		{
			// Compute scores
			IPGTerminalSet covered = EmptySet;
			int[] score = new int[sets.Length - 1]; // no error branch? then last set must be default
			for (int i = 0; i < score.Length; i++) {
				Debug.Assert(sets[i].Subtract(covered).Equals(sets[i]));
				score[i] = GetRelativeCostForSwitch(sets[i]);
			}

			// Consider highest scores first to figure out whether switch is 
			// justified, and which branches should be expressed with "case"s.
			bool should = false;
			int switchScore = -BaseCostForSwitch;
			for (;;) {
				int maxIndex = score.IndexOfMax(), maxScore = score[maxIndex];
				switchScore += maxScore;
				if (switchScore > 0)
					should = true;
				else if (maxScore < 0)
					break;
				casesToInclude.Add(maxIndex);
				score[maxIndex] = -1000000;
			}
			return should;
		}

		public virtual LNode GenerateSwitch(IPGTerminalSet[] branchSets, MSet<int> casesToInclude, LNode[] branchCode, LNode defaultBranch, LNode laVar)
		{
			Debug.Assert(branchSets.Length == branchCode.Length);

			RWList<LNode> stmts = new RWList<LNode>();
			for (int i = 0; i < branchSets.Length; i++) {
				if (casesToInclude.Contains(i)) {
					foreach (LNode value in GetCases(branchSets[i])) {
						stmts.Add(F.Call(S.Case, value));
						if (stmts.Count > 65535) // sanity check
							throw new InvalidOperationException("switch is too large to generate");
					}
					AddSwitchHandler(branchCode[i], stmts);
				}
			}

			if (!defaultBranch.IsIdNamed(S.Missing)) {
				stmts.Add(F.Call(S.Label, F.Id(S.Default)));
				AddSwitchHandler(defaultBranch, stmts);
			}

			return F.Call(S.Switch, (LNode)laVar, F.Braces(stmts.ToRVList()));
		}
		private void AddSwitchHandler(LNode branch, RWList<LNode> stmts)
		{
			stmts.SpliceAdd(branch, S.List);
			if (!branch.Calls(S.Goto, 1))
				stmts.Add(F.Call(S.Break));
		}

		public virtual LNode CreateRuleMethod(Rule rule, RVList<LNode> methodBody, bool recognizerMode)
		{
			return rule.CreateMethod(methodBody, recognizerMode);
		}

		public virtual LNode CallRuleAndSaveResult(RuleRef rref)
		{
			return rref.AutoSaveResult(F.Call(rref.Rule.Name, rref.Params));
		}
	}
}
