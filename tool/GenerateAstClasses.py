# generates the file Expr.cs based on AstDefinitions.txt


def generate_ast_classes(inputPath, outputPath, baseClassName):

	input = open(inputPath, "r", encoding='utf-8-sig')
	output = open(outputPath, "w")
	types = input.readlines()

	header = """namespace Lox
{{
	using System.Collections.Generic;

	abstract class {} 
	{{ 
		public abstract T Accept<T>(IVisitor<T> visitor);

""".format(baseClassName)

	# open namespace and base class
	output.write(header)

	# define visitor interface
	output.write("        public interface IVisitor<T> {\n")
	for typeName in [typeInfo.split(":")[0].strip() for typeInfo in types]:
		output.write("            T Visit{}{}({} {});\n".format(typeName, baseClassName, typeName, baseClassName.lower()))
	output.write("        }\n")

	# each line will become one subclass
	# form of type strings:
	# "Binary   : Expr left, Token op, Expr right"
	for typeInfo in types:
		className = typeInfo.split(":")[0].strip()
		fieldListing = typeInfo.split(":")[1].strip()
		fields = [f.strip() for f in fieldListing.split(",")]

		output.write("\n")
		# open class
		output.write("        public class {} : {} {{\n".format(className, baseClassName))

		# fields
		output.write("\n")
		for fieldType, fieldName in [f.split(" ") for f in fields]:
			output.write("            public {} {};\n".format(fieldType, fieldName))
		output.write("\n")

		# constructor
		output.write("            public {} ({}) {{\n".format(className, fieldListing))
		for field in fields:
			fieldName = field.split(" ")[1]
			output.write("                this.{} = {};\n".format(fieldName, fieldName))
		output.write("            }\n")

		#implement IVisitor
		output.write("\n")
		output.write("            public override T Accept<T>(IVisitor<T> visitor) {{ return visitor.Visit{}{}(this); }}\n".format(className, baseClassName))
		output.write("\n")

		# close class
		output.write("	    }\n")

	# close base class
	output.write("    }\n\n")

	# close namespace
	output.write("}")

	input.close()
	output.close()


generate_ast_classes("./ExprAstDefinition.txt", "../Expr.cs", "Expr")
generate_ast_classes("./StmtAstDefinition.txt", "../Stmt.cs", "Stmt")