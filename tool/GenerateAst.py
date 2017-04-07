# generates the file Expr.cs based on AstDefinitions.txt

inputFile = open("./AstDefinitions.txt", "r", encoding='utf-8-sig')
outputFile = open("../Expr.cs", "w")
types = inputFile.readlines()

header = """namespace Lox
{
    abstract class Expr 
    { 
        public abstract T Accept<T>(IVisitor<T> visitor);
"""

# open namespace and base class
outputFile.write(header)


# each line will become one subclass of Expr
# form of type strings:
# "Binary   : Expr left, Token op, Expr right"
for typeInfo in types:
    className = typeInfo.split(":")[0].strip()
    fieldListing = typeInfo.split(":")[1].strip()
    fields = [f.strip() for f in fieldListing.split(",")]

    outputFile.write("\n")
    # open class
    outputFile.write("        public class {} : {} {{\n".format(className, "Expr"))

    # fields
    outputFile.write("\n")
    for fieldType, fieldName in [f.split(" ") for f in fields]:
        outputFile.write("            public {} {};\n".format(fieldType, fieldName))
    outputFile.write("\n")

    # constructor
    outputFile.write("            public {} ({}) {{\n".format(className, fieldListing))
    for field in fields:
        fieldName = field.split(" ")[1]
        outputFile.write("                this.{} = {};\n".format(fieldName, fieldName))
    outputFile.write("            }\n")

	#implement IVisitor
    outputFile.write("\n")
    outputFile.write("            public override T Accept<T>(IVisitor<T> visitor) {{ return visitor.Visit{}Expr(this); }}\n".format(className))
    outputFile.write("\n")

    # close class
    outputFile.write("	    }\n")

# close base class
outputFile.write("    }\n\n")

# define visitor interface
outputFile.write("    interface IVisitor<T> {\n")
for typeName in [typeInfo.split(":")[0].strip() for typeInfo in types]:
    outputFile.write("        T Visit{}Expr(Expr.{} expr);\n".format(typeName, typeName))
outputFile.write("    }\n")

# close namespace
outputFile.write("}")

inputFile.close()
outputFile.close()
