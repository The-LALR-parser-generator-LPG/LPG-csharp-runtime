using System;
using System.Collections;
using System.Collections.Generic;

namespace LPG2.Runtime
{
   

//
// PrsStream holds an arraylist of tokens "lexed" from the input stream.
//
    public class PrsStream : ParseErrorCodes,IPrsStream
    {
    private ILexStream iLexStream;
    private int[] kindMap = null;
    private LPG2.Runtime.ArrayList<object> tokens = new LPG2.Runtime.ArrayList<object>();
    private LPG2.Runtime.ArrayList<object> adjuncts = new LPG2.Runtime.ArrayList<object>();
    private int index = 0;
    private int len = 0;

    public PrsStream()
    {
    }

    public PrsStream(ILexStream iLexStream)
    {
        this.iLexStream = iLexStream;
        if (iLexStream != null) iLexStream.setPrsStream(this);
        resetTokenStream();
    }

    public string[] orderedExportedSymbols()
    {
        return null;
    }

    public void remapTerminalSymbols(string[] ordered_parser_symbols, int eof_symbol)

    //throws UndefinedEofSymbolException,
    //    NullExportedSymbolsException,
    //    NullTerminalSymbolsException,
    //    UnimplementedTerminalsException

    {
        // SMS 12 Feb 2008
        // lexStream might be null, maybe only erroneously, but it has happened
        if (iLexStream == null)
            throw new NullReferenceException(
                "lpg.runtime.PrsStream.remapTerminalSymbols(..):  lexStream is null");

        string[] ordered_lexer_symbols = iLexStream.orderedExportedSymbols();
        if (ordered_lexer_symbols == null)
            throw new NullExportedSymbolsException();
        if (ordered_parser_symbols == null)
            throw new NullTerminalSymbolsException();
        ArrayList<int> unimplemented_symbols = new ArrayList<int>();
        if (ordered_lexer_symbols != ordered_parser_symbols)
        {
            kindMap = new int[ordered_lexer_symbols.Length];

            Dictionary<string,int> terminal_map = new Dictionary<string, int>();
            for (int i = 0; i < ordered_lexer_symbols.Length; i++)
                terminal_map[ordered_lexer_symbols[i]]=i;
            for (int i = 0; i < ordered_parser_symbols.Length; i++)
            {
                int k;
                if (terminal_map.TryGetValue(ordered_parser_symbols[i],out k))
                    kindMap[k] = i;
                else
                {
                    if (i == eof_symbol)
                        throw new UndefinedEofSymbolException();

                    unimplemented_symbols.add(i);
                }
            }
        }

        if (unimplemented_symbols.size() > 0)
            throw new UnimplementedTerminalsException(unimplemented_symbols);
    }

    public  int mapKind(int kind) {
        return (kindMap == null ? kind : kindMap[kind]);
    }

    public void resetTokenStream()
    {
        tokens = new ArrayList<object>();
        index = 0;

        adjuncts = new ArrayList<object>();
    }

    public void setLexStream(ILexStream lexStream)
    {
        this.iLexStream = lexStream;
        resetTokenStream();
    }

    /**
     * @deprecated function
     */
    public void resetLexStream(LexStream lexStream)
    {
        this.iLexStream = lexStream;
        if (lexStream != null) lexStream.setPrsStream(this);
    }

    public void makeToken(int startLoc, int endLoc, int kind)
    {
        Token token = new Token(this, startLoc, endLoc, mapKind(kind));
        token.setTokenIndex(tokens.Count);
        tokens.Add(token);
        token.setAdjunctIndex(adjuncts.size());
    }

    public void removeLastToken()
    {
        
        int last_index = tokens.Count - 1;
        Token token = (Token) tokens[(last_index)];
        int adjuncts_size = adjuncts.size();
        while (adjuncts_size > token.getAdjunctIndex())
            adjuncts.remove(--adjuncts_size);
        tokens.remove(last_index);
    }

    public int makeErrorToken(int firsttok, int lasttok, int errortok, int kind)
    {
        int index = tokens.Count; // the next index

        //
        // Note that when creating an error token, we do not remap its kind.
        // Since this is not a lexical operation, it is the responsibility of
        // the calling program (a parser driver) to pass to us the proper kind
        // that it wants for an error token.
        //
        Token token = new ErrorToken(getIToken(firsttok),
            getIToken(lasttok),
            getIToken(errortok),
            getStartOffset(firsttok),
            getEndOffset(lasttok),
            kind);
        token.setTokenIndex(tokens.Count);
   
        tokens.Add(token);
        token.setAdjunctIndex(adjuncts.size());

        return index;
    }

    public void addToken(IToken token)
    {
        token.setTokenIndex(tokens.Count);
        tokens.Add(token);
        token.setAdjunctIndex(adjuncts.size());
    }

    public void makeAdjunct(int startLoc, int endLoc, int kind)
    {
        int token_index = tokens.Count - 1; // index of last token processed
        Adjunct adjunct = new Adjunct(this, startLoc, endLoc, mapKind(kind));
        adjunct.setAdjunctIndex(adjuncts.size());
        adjunct.setTokenIndex(token_index);
        adjuncts.add(adjunct);
    }

    public void addAdjunct(IToken adjunct)
    {
        int token_index = tokens.Count - 1; // index of last token processed
        adjunct.setTokenIndex(token_index);
        adjunct.setAdjunctIndex(adjuncts.size());
        adjuncts.add(adjunct);
    }

    public string getTokenText(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return t.toString();
    }

    public int getStartOffset(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return t.getStartOffset();
    }

    public int getEndOffset(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return t.getEndOffset();
    }

    public int getTokenLength(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return t.getEndOffset() - t.getStartOffset() + 1;
    }

    public int getLineNumberOfTokenAt(int i)
    {
       
        IToken t = (IToken) tokens.get(i);
        return iLexStream.getLineNumberOfCharAt(t.getStartOffset());
    }

    public int getEndLineNumberOfTokenAt(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return iLexStream.getLineNumberOfCharAt(t.getEndOffset());
    }

    public int getColumnOfTokenAt(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return iLexStream.getColumnOfCharAt(t.getStartOffset());
    }

    public int getEndColumnOfTokenAt(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return iLexStream.getColumnOfCharAt(t.getEndOffset());
    }

    public string[] orderedTerminalSymbols()
    {
        return null;
    }

    public int getLineOffset(int i)
    {
        return iLexStream.getLineOffset(i);
    }

    public int getLineCount()
    {
        return iLexStream.getLineCount();
    }

    public int getLineNumberOfCharAt(int i)
    {
        return iLexStream.getLineNumberOfCharAt(i);
    }

    public int getColumnOfCharAt(int i)
    {
        return getColumnOfCharAt(i);
    }

    /**
     * @deprecated replaced by {@link #getFirstRealToken()}
     *
     */
    public int getFirstErrorToken(int i)
    {
        return getFirstRealToken(i);
    }

    public int getFirstRealToken(int i)
    {
        while (i >= len)
            i = ((ErrorToken) tokens.get(i)).getFirstRealToken().getTokenIndex();
        return i;
    }

    /**
     * @deprecated replaced by {@link #getLastRealToken()}
     *
     */
    public int getLastErrorToken(int i)
    {
        return getLastRealToken(i);
    }

    public int getLastRealToken(int i)
    {
        while (i >= len)
            i = ((ErrorToken) tokens.get(i)).getLastRealToken().getTokenIndex();
        return i;
    }

    // TODO: should this function throw an exception instead of returning null?
    public char[] getInputChars()
    {
        return (iLexStream is LexStream
            ? ((LexStream) iLexStream).getInputChars()
            : null);
    }

    // TODO: should this function throw an exception instead of returning null?
    public byte[] getInputBytes()
    {
        return (iLexStream is Utf8LexStream
            ? ((Utf8LexStream) iLexStream).getInputBytes()
            : null);
    }

    public string toString(int first_token, int last_token)
    {
        return toString((IToken) tokens.get(first_token), (IToken) tokens.get(last_token));
    }

    public string toString(IToken t1, IToken t2)
    {
        return iLexStream.toString(t1.getStartOffset(), t2.getEndOffset());
    }

    public int getSize()
    {
        return tokens.Count;
    }

    /**
     * @deprecated replaced by {@link #setStreamLength()}
     *
     */
    public void setSize()
    {
        len = tokens.Count;
    }

    //
    // This function returns the index of the token element
    // containing the offset specified. If such a token does
    // not exist, it returns the negation of the index of the 
    // element immediately preceding the offset.
    //
    public int getTokenIndexAtCharacter(int offset)
    {
        int low = 0,
            high = tokens.Count;
        while (high > low)
        {
            int mid = (high + low) / 2;
            IToken mid_element = (IToken) tokens.get(mid);
            if (offset >= mid_element.getStartOffset() &&
                offset <= mid_element.getEndOffset())
                return mid;
            else if (offset < mid_element.getStartOffset())
                high = mid;
            else low = mid + 1;
        }

        return -(low - 1);
    }

    public IToken getTokenAtCharacter(int offset)
    {
        int tokenIndex = getTokenIndexAtCharacter(offset);
        return (tokenIndex < 0) ? null : getTokenAt(tokenIndex);
    }

    public IToken getTokenAt(int i)
    {
        return (IToken) tokens.get(i);
    }

    public IToken getIToken(int i)
    {
        return (IToken) tokens.get(i);
    }

    public ArrayList getTokens()
    {
        return tokens;
    }

    public int getStreamIndex()
    {
        return index;
    }

    public int getStreamLength()
    {
        return len;
    }

    public void setStreamIndex(int index)
    {
        this.index = index;
    }

    public void setStreamLength()
    {
        this.len = tokens.Count;
    }

    public void setStreamLength(int len)
    {
        this.len = len;
    }

    public ILexStream getILexStream()
    {
        return iLexStream;
    }

    /**
     * @deprecated replaced by {@link #getILexStream()}
     */
    public ILexStream getLexStream()
    {
        return iLexStream;
    }

    public void dumpTokens()
    {
        if (getSize() <= 2) return;
        Console.Out.WriteLine(" Kind \tOffset \tLen \tLine \tCol \tText\n");
        for (int i = 1; i < getSize() - 1; i++) dumpToken(i);
    }

    public void dumpToken(int i)
    {
       Console.Out.Write(" (" + getKind(i) + ")");
       Console.Out.Write(" \t" + getStartOffset(i));
       Console.Out.Write(" \t" + getTokenLength(i));
       Console.Out.Write(" \t" + getLineNumberOfTokenAt(i));
       Console.Out.Write(" \t" + getColumnOfTokenAt(i));
       Console.Out.Write(" \t" + getTokenText(i));
        Console.Out.WriteLine();
    }

    private IToken[] getAdjuncts(int i)
    {
        int start_index = ((IToken) tokens.get(i)).getAdjunctIndex(),
            end_index = (i + 1 == tokens.Count
                ? adjuncts.size()
                : ((IToken) tokens.get(getNext(i))).getAdjunctIndex()),
            size = end_index - start_index;
        IToken[] slice = new IToken[size];
        for (int j = start_index, k = 0; j < end_index; j++, k++)
            slice[k] = (IToken) adjuncts.get(j);
        return slice;
    }

    //
    // Return an iterator for the adjuncts that follow token i.
    //
    public IToken[] getFollowingAdjuncts(int i)
    {
        return getAdjuncts(i);
    }

    public IToken[] getPrecedingAdjuncts(int i)
    {
        return getAdjuncts(getPrevious(i));
    }

    public ArrayList getAdjuncts()
    {
        return adjuncts;
    }

    //
    // Methods that implement the TokenStream Interface
    //
    public int getToken()
    {
        index = getNext(index);
        return index;
    }

    public int getToken(int end_token)
    {
        return index = (index < end_token ? getNext(index) : len - 1);
    }

    public int getKind(int i)
    {
        IToken t = (IToken) tokens.get(i);
        return t.getKind();
    }

    public int getNext(int i)
    {
        return (++i < len ? i : len - 1);
    }

    public int getPrevious(int i)
    {
        return (i <= 0 ? 0 : i - 1);
    }

    public string getName(int i)
    {
        return getTokenText(i);
    }

    public int peek()
    {
        return getNext(index);
    }

    public void reset(int i)
    {
        index = getPrevious(i);
    }

    public void reset()
    {
        index = 0;
    }

    public int badToken()
    {
        return 0;
    }

    public int getLine(int i)
    {
        return getLineNumberOfTokenAt(i);
    }

    public int getColumn(int i)
    {
        return getColumnOfTokenAt(i);
    }

    public int getEndLine(int i)
    {
        return getEndLineNumberOfTokenAt(i);
    }

    public int getEndColumn(int i)
    {
        return getEndColumnOfTokenAt(i);
    }

    public bool afterEol(int i)
    {
        return (i < 1 ? true : getEndLineNumberOfTokenAt(i - 1) < getLineNumberOfTokenAt(i));
    }

    public string getFileName()
    {
        return iLexStream.getFileName();
    }

    //
    // Here is where we report errors.  The default method is simply to print the error message to the console.
    // However, the user may supply an error message handler to process error messages.  To support that
    // a message handler interface is provided that has a single method handleMessage().  The user has his
    // error message handler class implement the IMessageHandler interface and provides an object of this type
    // to the runtime using the setMessageHandler(errorMsg) method. If the message handler object is set, 
    // the reportError methods will invoke its handleMessage() method.
    //
    // IMessageHandler errMsg = null; // the error message handler object is declared in LexStream
    //
    public void setMessageHandler(IMessageHandler errMsg)
    {
        iLexStream.setMessageHandler(errMsg);
    }

    public IMessageHandler getMessageHandler()
    {
        return iLexStream.getMessageHandler();
    }

    public void reportError(int errorCode, int leftToken, int rightToken, string errorInfo)
    {
        // if (errorCode == DELETION_CODE ||
        //    errorCode == MISPLACED_CODE) tokenText = "";
        reportError(errorCode,
            leftToken,
            0,
            rightToken,
            errorInfo == null ? null : new string[] {errorInfo});
    }

    public void reportError(int errorCode, int leftToken, int rightToken, string[] errorInfo)
    {
        // if (errorCode == DELETION_CODE ||
        //    errorCode == MISPLACED_CODE) tokenText = "";
        reportError(errorCode,
            leftToken,
            0,
            rightToken,
            errorInfo);
    }

    public void reportError(int errorCode, int leftToken, int errorToken, int rightToken, string errorInfo)
    {
        reportError(errorCode,
            leftToken,
            errorToken,
            rightToken,
            errorInfo == null ? null : new string[] {errorInfo});
    }

    public void reportError(int errorCode, int leftToken, int errorToken, int rightToken, string[] errorInfo)
    {
        iLexStream.reportLexicalError(errorCode,
            getStartOffset(leftToken),
            getEndOffset(rightToken),
            getStartOffset(errorToken),
            getEndOffset(errorToken),
            errorInfo == null ? new string[] { } : errorInfo);
    }
    }
}