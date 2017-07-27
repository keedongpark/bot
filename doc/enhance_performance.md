# Enhance Performance

 - Act�� �ڵ� ���� 
 - �ѹ��� ���� 
 - Reflection���� ȣ��
   - Invoke

# Compile

https://stackoverflow.com/questions/32769630/how-to-compile-a-c-sharp-file-with-roslyn-programmatically

MetadataReference assembly
typeof(DataSetExtensions).Assembly.Location

http://www.csharpstudy.com/DevNote/Article/18

- �ѱ� ���� 
- �� �� ���� �Ǿ� ����

# Use 

Assembly assembly = Assembly.LoadFile(@"C:\dyn.dll");
Type     type     = assembly.GetType("TestRunner");
var      obj      = Activator.CreateInstance(type);

// Alternately you could get the MethodInfo for the TestRunner.Run method
type.InvokeMember("Run", 
                  BindingFlags.Default | BindingFlags.InvokeMethod, 
                  null,
                  obj,
                  null);

# In memory compilation and use

http://josephwoodward.co.uk/2016/12/in-memory-c-sharp-compilation-using-roslyn

 var stream = new MemoryStream();
    var emitResult = compilation.Emit(stream);
    
    if (emitResult.Success){
        stream.Seek(0, SeekOrigin.Begin);
        assembly = Assembly.Load(stream.GetBuffer());
    }

	22JArray item = (JArray)channel["item"];
23item.Add("Item 1");
24item.Add("Item 2");