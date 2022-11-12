using System;
using NRO_Server.Application.Interfaces.Character;
using NRO_Server.Application.IO;
using NRO_Server.DatabaseManager.Player;
using NRO_Server.Model.Character;

namespace NRO_Server.Application.Handlers.Character
{
    public class MagicTreeHandler : IMagicTreeHandler
    {
        private readonly MagicTree _magicTree;

        public MagicTreeHandler(MagicTree magicTree)
        {
            _magicTree = magicTree;
        }

        public void Update(int id)
        {
            lock (_magicTree)
            {
                switch (id)
                {
                    //Save date
                    case 0:
                    {
                        Flush();
                        break;
                    }
                    //Update tree
                    case 1:
                    {
                        if (_magicTree.Seconds == 0) return;
                        lock (_magicTree)
                        {
                            if (_magicTree.Seconds > ServerUtils.CurrentTimeMillis())
                            {
                                HandleNgoc();
                            }
                            else if (_magicTree.IsUpdate)
                            {
                                _magicTree.IsUpdate = false;
                                _magicTree.Level++;
                                switch (_magicTree.Level)
                                {
                                    case < 8:
                                        _magicTree.NpcId++;
                                        break;
                                    case >= 10:
                                        _magicTree.Level = 10;
                                        break;
                                }
                        
                                _magicTree.MaxPea += 2; 
                                _magicTree.Peas = _magicTree.MaxPea;
                                _magicTree.Seconds = 0;
                                _magicTree.Diamond = 0;
                                MagicTreeDB.Update(_magicTree);
                            }
                            else
                            {
                                if (_magicTree.Peas >= _magicTree.MaxPea) return;
                                {
                                    _magicTree.Peas++;
                                    if (_magicTree.Peas == _magicTree.MaxPea)
                                    {
                                        _magicTree.Seconds = 0;
                                        _magicTree.Diamond = 0;
                                    }
                                    else
                                    {
                                        HandleNgoc();
                                        _magicTree.Seconds = 60000 * _magicTree.Level + ServerUtils.CurrentTimeMillis();
                                    }
                                }
                            }
                        }
                        break;
                    }
                }
            }
        }

        public void HandleNgoc()
        {
            var minePea = _magicTree.MaxPea - _magicTree.Peas;
            var level = 1;
            if (!_magicTree.IsUpdate)
            {
                if (_magicTree.Level >= 5) level = 2;
                if (_magicTree.Level >= 7) level = 3;
                _magicTree.Diamond = minePea * level + (level*2 + 5);
            }
            else
            {
                if (_magicTree.Seconds != 0 && _magicTree.Seconds > ServerUtils.CurrentTimeMillis())
                {
                    var minutes = (_magicTree.Seconds - ServerUtils.CurrentTimeMillis()) / 60000;
                    var per = 10 * _magicTree.Level + (_magicTree.Level - 1) * minutes/10;
                    switch (_magicTree.Level)
                    {
                        case 8:
                            per -= 3000;
                            break;
                        case 9:
                            per -= 15000;
                            break;
                    }

                    _magicTree.Diamond = (int)per;
                }
                else
                {
                    _magicTree.Diamond = 1;
                }
            }
            
        }

        public void Flush()
        {
            MagicTreeDB.Update(_magicTree);
        }

    }
}