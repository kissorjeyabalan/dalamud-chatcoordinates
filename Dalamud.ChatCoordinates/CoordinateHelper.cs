namespace ChatCoordinates
{
    public static class CoordinateHelper
    {
        public static int ConvertMapCoordinateToRawPosition(float pos, float scale)
        {
            float num = scale / 100f;
            return (int) ((float) ((pos - 1.0) * num / 41.0 * 2048.0 - 1024.0) / num * 1000f);
        }

        public static float MapMarkerToMapPos(float pos, float scale)
        {
            var num = scale / 100f;
            var rawPos = (int) ((float) (pos - 1024.0) / num * 1000f);
            return RawPosToMapPos(rawPos, scale);
        }

        public static float RawPosToMapPos(int pos, float scale)
        {
            var num = scale / 100f;
            return (float) ((pos / 1000f * num + 1024.0) / 2048.0 * 41.0 / num + 1.0);
        }
    }
}