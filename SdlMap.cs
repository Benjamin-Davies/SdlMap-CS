using System;
using static SDL2.SDL;

namespace SdlMapCS
{
    public static class SdlMap
    {
        private static void RunLoop(MapView view)
        {
            bool mouseDown = false;
            bool dirty = false;
            uint lastClick = 0;
            int zoomdf = 0;

            while (true)
            {
                while (SDL_PollEvent(out SDL_Event @event) > 0)
                {
                    switch (@event.type)
                    {
                        case SDL_EventType.SDL_MOUSEBUTTONUP:
                            mouseDown = false;
                            break;
                        case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                            if (@event.button.button == 1)
                            {
                                mouseDown = true;
                                if (SDL_GetTicks() - lastClick < 250)
                                {
                                    lastClick = 0;
                                    view.ZoomAt(@event.button.x, @event.button.y);
                                    dirty = true;
                                }
                                lastClick = SDL_GetTicks();
                            }
                            break;
                        case SDL_EventType.SDL_MOUSEWHEEL:
                            zoomdf += @event.wheel.y;
                            if (zoomdf >= 5)
                            {
                                zoomdf = 0;
                                view.ZoomIn();
                                dirty = true;
                            }
                            else if (zoomdf <= -5)
                            {
                                zoomdf = 0;
                                view.ZoomOut();
                                dirty = true;
                            }
                            break;
                        case SDL_EventType.SDL_MOUSEMOTION:
                            if (mouseDown)
                            {
                                view.MoveBy(-@event.motion.xrel, -@event.motion.yrel);
                                dirty = true;
                            }
                            break;
                        case SDL_EventType.SDL_KEYDOWN:
                            switch (@event.key.keysym.sym)
                            {
                                case SDL_Keycode.SDLK_ESCAPE:
                                    return;
                                case SDL_Keycode.SDLK_PLUS:
                                case SDL_Keycode.SDLK_EQUALS:
                                    view.ZoomIn();
                                    dirty = true;
                                    break;
                                case SDL_Keycode.SDLK_MINUS:
                                    view.ZoomOut();
                                    dirty = true;
                                    break;
                            }
                            break;
                        case SDL_EventType.SDL_WINDOWEVENT:
                            switch(@event.window.windowEvent)
                            {
                                case SDL_WindowEventID.SDL_WINDOWEVENT_RESIZED:
                                    view.Resize(@event.window.data1, @event.window.data2);
                                    break;
                                case SDL_WindowEventID.SDL_WINDOWEVENT_EXPOSED:
                                    dirty = true;
                                    break;
                            }
                            break;
                        case SDL_EventType.SDL_QUIT:
                            return;
                    }
                }
                if (dirty)
                {
                    view.UpdateBounds();
                    dirty = view.Tiles.Work();
                    view.Render();
                }
            }
        }

        public static int Main(string[] args)
        {
            if (SDL_Init(SDL_INIT_VIDEO) > 0)
            {
                Console.WriteLine("SDL_Init failed {0}", SDL_GetError());
                return -1;
            }

            int width = 600, height = 800;
            int zoom = 12;
            var window = SDL_CreateWindow("SDLmap", SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED, width, height, SDL_WindowFlags.SDL_WINDOW_SHOWN | SDL_WindowFlags.SDL_WINDOW_RESIZABLE);

            var view = new MapView(window, width, height, zoom);
            view.CenterCoords(48.4284, -123.3656);

            RunLoop(view);

            return 0;
        }
    }
}
