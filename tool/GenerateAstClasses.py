from jinja2 import Template

# generates file Expr.cs based on ExprAstDefinition.txt
# generates file Stmt.cs based on StmtAstDefinition.txt

template = Template("""namespace Lox
{
    using System.Collections.Generic;
    
    abstract class {{baseclass}}
    {
        public abstract T Accept<T>(IVisitor<T> visitor);
        
        public interface IVisitor<T> {
        {% for type in types %}
            T Visit{{type.className}}{{baseclass}}({{type.className}} {{baseclass|lower}});
        {%- endfor %}  
               
        }
        
        {% for type in types %}
        public class {{type.className}} : {{baseclass}}
        {
            {%- for field in type.fields %}
            public readonly {{field.type}} {{field.name}};
            {%- endfor %}
            
            public {{type.className}} ({%- for field in type.fields %} {{field.type}} {{field.name}}{%if not loop.last%},{%endif%} {%- endfor %})
            {
                {%- for field in type.fields %}
                this.{{field.name}} = {{field.name}};
                {%- endfor %}                
            }
            
            public override T Accept<T>(IVisitor<T> visitor) { return visitor.Visit{{type.className}}{{baseclass}}(this); }
        }
        
        {%- endfor %}
    }  
}
""")


def generate_ast_classes(inputPath, outputPath, baseClassName):

    with open(inputPath, "r", encoding="utf-8-sig") as input:
        typesLines = input.readlines()

    types = []
    # each line will become one subclass
    # form of type strings:
    # "Binary   : Expr left, Token op, Expr right"
    for life in typesLines:
        className, fieldListing = life.split(":")
        fields = []
        for fieldType, fieldName in [f.strip().split(" ") for f in fieldListing.split(",")]:
            fields.append({"type": fieldType, "name": fieldName})

        types.append({"className": className.strip(), "fields": fields})

    content = template.render(types=types, baseclass=baseClassName)

    with open(outputPath, "w", encoding="utf8") as output:
        output.write(content)


generate_ast_classes(".\ExprAstDefinition.txt", "..\Expr.cs", "Expr")
generate_ast_classes(".\StmtAstDefinition.txt", "..\Stmt.cs", "Stmt")
