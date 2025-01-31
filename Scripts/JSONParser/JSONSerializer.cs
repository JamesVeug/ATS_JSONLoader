public interface JSONSerializer<FromType,ToType>
{
    // Invoked from ImportExportUtils.TryGetConvertInterface()
    public ToType Convert(FromType from);
}