# GenericRuleParser
A parser for simple rules in C#

This parses string rules in the form of FIELDNAME OPERATOR 'VALUE' [CONJUNCTION FIELDNAME_n OPERATOR_n 'VALUE_n'] where:

* The space characters (the space character, ASCII 32) is the delimter between parts of the rule(s)
* FIELNAME and OPERATOR contain no spaces
* VALUE is always contained by single quotes and contains no single quotes - but it may contain spaces
* CONJUNCTION is either AND or OR

Some simple examples are

  x = '1'
  foo equals 'bar' AND baz greaterthan '0'

The parser also supports grouping rules with parentheses, so that you could also do

  x = '1' OR (Foo equals 'bar' AND baz greaterthan '0')
or
  (x = '1' OR Foo equals 'bar') AND baz greaterthan '0'
or
  (x = '1') OR (Foo equals 'bar' AND (baz greaterthan '0'))

The parser DOES NOT support any form of precedence, so trying 

  x = '1' OR Foo equals 'bar' AND baz greaterthan '0'

throws and exception due to mismatched conjuctions
